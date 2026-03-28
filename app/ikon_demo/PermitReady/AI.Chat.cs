public partial class IkonDemoApp
{
    private record ChatReply(string Message);

    // ── Initialize chat: uses cache to avoid regenerating on revisit ──────
    private async Task InitializeChatAsync(PermitReady.StoredApplication app, bool forceRefresh = false)
    {
        _chatMessages.Value  = [];
        _chatInput.Value     = "";
        _chatStreaming.Value  = false;
        _showAddSource.Value = false;
        _sourceError.Value   = "";

        // Restore source context from cache (preserves added URLs/files)
        if (!forceRefresh && _sourcesCache.TryGetValue(app.ApplicationId, out var cachedSources))
            _chatSources.Value = cachedSources.ToList();
        else
        {
            _chatSources.Value = [BuildPreScreeningSource(app)];
            _sourcesCache[app.ApplicationId] = _chatSources.Value.ToList();
        }

        _chatContext = new KernelContext();

        // Restore initial assessment from cache
        if (!forceRefresh && _assessmentCache.TryGetValue(app.ApplicationId, out var cached))
        {
            _chatMessages.Value = [new PermitReady.ChatMessage(
                PermitReady.ChatRole.Assistant, cached, DateTime.UtcNow)];
            return;
        }

        // Generate fresh assessment using fast Haiku model
        _chatStreaming.Value = true;

        try
        {
            var systemPrompt = BuildSystemPrompt(app, _chatSources.Value);
            var cmd = $"""
                Provide a concise structured initial assessment for this {app.Input.Category.DisplayName()} application:

                **Overview** — applicant identity and permit type (2 sentences)
                **Verdict** — completeness score, risk score, routing decision
                **Key Findings** — top 3-5 issues or positives (bullet points)
                **Recommended Action** — single clear next step for the officer

                Be direct and professional. Use markdown.
                """;

            var (reply, ctx) = await Emerge.Run<ChatReply>(
                LLMModel.Claude45Haiku, _chatContext, pass =>
                {
                    pass.SystemPrompt = systemPrompt;
                    pass.Command      = cmd;
                    pass.Temperature  = 0.15;
                }).FinalAsync();

            _chatContext = ctx;
            var msg = reply.Message;

            _chatMessages.Value = [new PermitReady.ChatMessage(
                PermitReady.ChatRole.Assistant, msg, DateTime.UtcNow)];

            // Cache for subsequent visits
            _assessmentCache[app.ApplicationId] = msg;
        }
        catch (Exception ex)
        {
            _chatMessages.Value = [new PermitReady.ChatMessage(
                PermitReady.ChatRole.Assistant,
                $"⚠ Failed to generate assessment: {ex.Message}",
                DateTime.UtcNow)];
        }
        finally
        {
            _chatStreaming.Value = false;
        }
    }

    // ── Send user message ─────────────────────────────────────────────────
    private async Task SendChatMessageAsync(PermitReady.StoredApplication app)
    {
        var msg = _chatInput.Value.Trim();
        if (string.IsNullOrEmpty(msg) || _chatStreaming.Value) return;

        _chatInput.Value    = "";
        _chatMessages.Value = [.. _chatMessages.Value,
            new PermitReady.ChatMessage(PermitReady.ChatRole.User, msg, DateTime.UtcNow)];
        _chatStreaming.Value = true;

        try
        {
            var systemPrompt = BuildSystemPrompt(app, _chatSources.Value);

            var (reply, ctx) = await Emerge.Run<ChatReply>(
                LLMModel.Claude46Sonnet, _chatContext, pass =>
                {
                    pass.SystemPrompt = systemPrompt;
                    pass.Command      = msg;
                    pass.Temperature  = 0.3;
                }).FinalAsync();

            _chatContext = ctx;
            var aiMsg = reply.Message;

            _chatMessages.Value = [.. _chatMessages.Value,
                new PermitReady.ChatMessage(PermitReady.ChatRole.Assistant, aiMsg, DateTime.UtcNow)];

            // Append to source cache (preserves context on revisit)
            _sourcesCache[app.ApplicationId] = _chatSources.Value.ToList();

            // Audit log
            _chatAuditLog.Value = [.. _chatAuditLog.Value,
                new PermitReady.ChatAuditEntry(app.ApplicationId, DateTime.UtcNow, msg, aiMsg)];
        }
        catch (Exception ex)
        {
            _chatMessages.Value = [.. _chatMessages.Value,
                new PermitReady.ChatMessage(PermitReady.ChatRole.Assistant,
                    $"Error: {ex.Message}", DateTime.UtcNow)];
        }
        finally
        {
            _chatStreaming.Value = false;
        }
    }

    // ── View document in chat ─────────────────────────────────────────────
    private async Task ViewDocumentInChatAsync(string docName, PermitReady.StoredApplication app)
    {
        var docType = InferDocType(docName);
        _chatMessages.Value = [.. _chatMessages.Value,
            new PermitReady.ChatMessage(PermitReady.ChatRole.User,
                $"📄 Review document: **{docName}**", DateTime.UtcNow)];
        _chatStreaming.Value = true;

        try
        {
            var cmd = $"""
                Review document: {docName} (type: {docType})
                For this {app.Input.Category.DisplayName()} application, provide:
                1. What to verify in this document
                2. Key fields to check
                3. Common red flags
                4. Specific questions for the officer
                """;

            var (reply, ctx) = await Emerge.Run<ChatReply>(
                LLMModel.Claude45Haiku, _chatContext, pass =>
                {
                    pass.Command     = cmd;
                    pass.Temperature = 0.2;
                }).FinalAsync();

            _chatContext = ctx;
            _chatMessages.Value = [.. _chatMessages.Value,
                new PermitReady.ChatMessage(PermitReady.ChatRole.Assistant, reply.Message, DateTime.UtcNow)];
        }
        catch (Exception ex)
        {
            _chatMessages.Value = [.. _chatMessages.Value,
                new PermitReady.ChatMessage(PermitReady.ChatRole.Assistant,
                    $"Error: {ex.Message}", DateTime.UtcNow)];
        }
        finally
        {
            _chatStreaming.Value = false;
        }
    }

    private static string InferDocType(string filename)
    {
        var l = filename.ToLowerInvariant();
        if (l.Contains("passport"))                           return "passport copy";
        if (l.Contains("marriage"))                           return "marriage certificate";
        if (l.Contains("birth"))                              return "birth certificate";
        if (l.Contains("contract") || l.Contains("employ"))  return "employment contract";
        if (l.Contains("degree")  || l.Contains("diploma"))  return "academic degree";
        if (l.Contains("transcript"))                         return "academic transcript";
        if (l.Contains("accept")  || l.Contains("offer"))    return "acceptance letter";
        if (l.Contains("bank")    || l.Contains("statement")) return "bank statement";
        if (l.Contains("hosting") || l.Contains("research")) return "hosting agreement";
        if (l.Contains("salary")  || l.Contains("payslip"))  return "salary proof";
        if (l.Contains("insur"))                              return "insurance certificate";
        if (l.Contains("business"))                           return "business document";
        if (l.Contains("training")|| l.Contains("intern"))   return "training agreement";
        if (l.Contains("custody"))                            return "custody document";
        if (l.Contains("sponsor") || l.Contains("permit"))   return "sponsor's residence permit";
        if (l.Contains("exchange"))                           return "exchange agreement";
        return "supporting document";
    }

    // ── Fetch guidelines from Migri.fi ────────────────────────────────────
    private async Task FetchGuidelinesForCategoryAsync(PermitReady.PermitCategory category)
    {
        var url   = category.MigriFiUrl();
        var label = $"Migri Guidelines: {category.ShortName()}";
        if (_chatSources.Value.Any(s => s.Title == label)) return;

        _sourcesFetching.Value = true;
        _sourceError.Value     = "";
        _showAddSource.Value   = false;

        try
        {
            using var scraper = new WebScraper(WebScraperModel.Jina);
            var result = await scraper.ScrapeSinglePageAsync(new SinglePageScrapeConfig
            {
                Url          = url,
                OutputFormat = WebScraperOutputFormat.Text,
                Timeout      = TimeSpan.FromSeconds(30),
            });

            var content = string.IsNullOrWhiteSpace(result.Content)
                ? $"Official Migri.fi guidelines for {category.DisplayName()}. See: {url}"
                : (result.Content.Length > 6000 ? result.Content[..6000] + "\n[truncated]" : result.Content);

            _chatSources.Value = [.. _chatSources.Value,
                new PermitReady.AnalysisSource(PermitReady.SourceType.Url, label, content)];
            _sourcesCache[_selectedAppId.Value ?? ""] = _chatSources.Value.ToList();

            _chatMessages.Value = [.. _chatMessages.Value,
                new PermitReady.ChatMessage(PermitReady.ChatRole.Assistant,
                    $"✅ Migri.fi guidelines for **{category.DisplayName()}** loaded. Ask me anything about the requirements.",
                    DateTime.UtcNow)];
        }
        catch (Exception ex)
        {
            _sourceError.Value = $"Could not fetch guidelines: {ex.Message}";
        }
        finally
        {
            _sourcesFetching.Value = false;
        }
    }

    // ── Add URL source ────────────────────────────────────────────────────
    private async Task AddUrlSourceAsync()
    {
        var url = _addSourceText.Value.Trim();
        if (string.IsNullOrEmpty(url)) return;
        if (!url.StartsWith("http://") && !url.StartsWith("https://")) url = "https://" + url;

        _sourcesFetching.Value = true;
        _sourceError.Value     = "";

        try
        {
            using var scraper = new WebScraper(WebScraperModel.Jina);
            var result = await scraper.ScrapeSinglePageAsync(new SinglePageScrapeConfig
            {
                Url = url, OutputFormat = WebScraperOutputFormat.Text, Timeout = TimeSpan.FromSeconds(30),
            });

            var title   = string.IsNullOrWhiteSpace(result.Title) ? url : result.Title;
            var content = result.Content.Length > 6000 ? result.Content[..6000] + "\n[truncated]" : result.Content;

            _chatSources.Value    = [.. _chatSources.Value,
                new PermitReady.AnalysisSource(PermitReady.SourceType.Url, title, content)];
            _addSourceText.Value  = "";
            _addSourceTitle.Value = "";
            _showAddSource.Value  = false;
        }
        catch (Exception ex) { _sourceError.Value = $"Could not fetch URL: {ex.Message}"; }
        finally { _sourcesFetching.Value = false; }
    }

    // ── Add text source ───────────────────────────────────────────────────
    private void AddTextSource()
    {
        var text  = _addSourceText.Value.Trim();
        var title = _addSourceTitle.Value.Trim();
        if (string.IsNullOrEmpty(text)) return;
        if (string.IsNullOrEmpty(title)) title = $"Note {_chatSources.Value.Count}";

        _chatSources.Value    = [.. _chatSources.Value,
            new PermitReady.AnalysisSource(PermitReady.SourceType.Text, title, text)];
        _addSourceText.Value  = "";
        _addSourceTitle.Value = "";
        _showAddSource.Value  = false;
    }

    // ── Add file source ───────────────────────────────────────────────────
    private async Task AddFileSourceAsync(FileUploadCompleteArgs args)
    {
        _sourcesFetching.Value = true;
        _sourceError.Value     = "";

        try
        {
            var content = ExtractPdfText(args.LocalTempFilePath!);
            if (content.Length > 8000) content = content[..8000] + "\n[truncated]";

            var title = !string.IsNullOrWhiteSpace(_addSourceTitle.Value)
                ? _addSourceTitle.Value.Trim() : args.FileName;

            _chatSources.Value    = [.. _chatSources.Value,
                new PermitReady.AnalysisSource(PermitReady.SourceType.File, title, content)];
            _showAddSource.Value  = false;
            _addSourceTitle.Value = "";
        }
        catch (Exception ex) { _sourceError.Value = $"Could not read file: {ex.Message}"; }
        finally { _sourcesFetching.Value = false; }
    }

    // ── Build pre-screening context source ───────────────────────────────
    private static PermitReady.AnalysisSource BuildPreScreeningSource(PermitReady.StoredApplication app)
    {
        var input  = app.Input;
        var result = app.Result;
        var sb     = new System.Text.StringBuilder();

        sb.AppendLine($"APPLICATION: {app.ApplicationId} | PERMIT: {input.Category.DisplayName()} | GROUP: {input.Group}");
        sb.AppendLine($"SUBMITTED: {app.SubmittedAt:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine();
        sb.AppendLine($"APPLICANT: {input.FullName} ({input.Nationality}) | Email: {input.Email}");

        if (input.PassportExpiry.HasValue)
        {
            var days = (input.PassportExpiry.Value - DateTime.UtcNow).Days;
            sb.AppendLine($"Passport Expiry: {input.PassportExpiry.Value:yyyy-MM-dd} ({days}d remaining)");
        }

        if (input.Group == PermitReady.PermitGroup.Study)
        {
            sb.AppendLine($"STUDY — University: {input.UniversityName ?? "N/A"} | Programme: {input.ProgramName ?? "N/A"} | Funds: €{input.FundsAmount:N0}/mo (min €560)");
        }
        else if (input.Group == PermitReady.PermitGroup.Work)
        {
            sb.AppendLine($"WORK — Employer: {input.EmployerName ?? "N/A"} | Title: {input.JobTitle ?? "N/A"} | Salary: €{input.SalaryAmount:N0}/mo | Contract: {input.EmploymentContractRef ?? "N/A"}");
        }
        else
        {
            sb.AppendLine($"FAMILY — Sponsor: {input.SponsorName ?? "N/A"} | Permit: {input.SponsorPermitNumber ?? "N/A"}");
        }

        sb.AppendLine();
        sb.AppendLine($"SCORES — Completeness: {result.CompletenessScore}/100 | Risk: {result.RiskScore}/100");
        sb.AppendLine($"ROUTING: {result.Routing switch {
            PermitReady.RoutingType.FastTrack           => "FAST TRACK",
            PermitReady.RoutingType.SupplementRequested => "SUPPLEMENT REQUESTED",
            _                                           => "SPECIALIST REVIEW"
        }}");

        if (result.MissingItems.Count > 0)
            sb.AppendLine($"MISSING: {string.Join(" | ", result.MissingItems)}");
        if (result.RiskFlags.Count > 0)
            sb.AppendLine($"RISK FLAGS: {string.Join(" | ", result.RiskFlags)}");

        sb.AppendLine($"DOCUMENTS: {(input.UploadedDocuments.Count == 0 ? "None" : string.Join(", ", input.UploadedDocuments))}");

        return new PermitReady.AnalysisSource(PermitReady.SourceType.Text, "Pre-screening Assessment", sb.ToString());
    }

    // ── Build system prompt ───────────────────────────────────────────────
    private static string BuildSystemPrompt(
        PermitReady.StoredApplication app,
        List<PermitReady.AnalysisSource> sources)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"""
            You are an expert Finnish immigration officer assistant specialising in {app.Input.Category.DisplayName()} applications.
            You know the Finnish Aliens Act (301/2004), Migri procedures, and assessment criteria deeply.
            Help the officer: analyse application vs Finnish law, identify risks, suggest follow-up actions, reference regulations.
            Be concise, professional, structured. Use markdown. Never fabricate data not in the application.
            """);

        sb.AppendLine("CONTEXT:");
        foreach (var src in sources)
        {
            sb.AppendLine($"[{src.Title}]\n{src.Content}");
            sb.AppendLine("---");
        }

        return sb.ToString();
    }
}
