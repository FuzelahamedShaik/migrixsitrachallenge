public partial class IkonDemoApp
{
    private void RenderDetail(UIView view)
    {
        var app = _applications.Value.FirstOrDefault(a => a.ApplicationId == _selectedAppId.Value);
        if (app == null) { Navigate("dashboard"); return; }

        // Auto-mark as UnderReview when officer opens the application
        if (app.Status == PermitReady.ApplicationStatus.Screened)
            UpdateApplicationStatus(app.ApplicationId, PermitReady.ApplicationStatus.UnderReview);

        // Initialize chat when application first loads (messages empty)
        if (_chatMessages.Value.Count == 0 && !_chatStreaming.Value)
            _ = InitializeChatAsync(app);

        // ── Full-height side-by-side layout ──────────────────────────────
        view.Row(["h-full overflow-hidden"], content: view =>
        {
            // LEFT: Application Profile
            RenderDetailLeft(view, app);

            // Divider
            view.Box(["w-px bg-border shrink-0"]);

            // RIGHT: AI Analysis Assistant
            RenderChatPanel(view, app);
        });
    }

    // ── LEFT PANEL: Applicant profile ────────────────────────────────────

    private void RenderDetailLeft(UIView view, PermitReady.StoredApplication app)
    {
        var input  = app.Input;
        var result = app.Result;
        var group  = input.Group;

        view.ScrollArea(rootStyle: ["w-[420px] shrink-0 h-full"], content: view =>
        {
            view.Column(["p-5 gap-4 min-h-full"], content: view =>
            {
                // Back button
                view.Button([Button.GhostSm, "w-fit -ml-1.5 text-muted-foreground"],
                    "← Dashboard",
                    onClick: async () => Navigate("dashboard"));

                // App ID header
                view.Column(["gap-1"], content: view =>
                {
                    view.Row(["items-center gap-2 flex-wrap"], content: view =>
                    {
                        view.Text([Text.H3], $"Application {app.ApplicationId}");
                        view.Box([Badge.DefaultSm, "text-xs px-2 py-0.5"],
                            content: v => v.Row(["items-center gap-1"], content: v =>
                            {
                                v.Icon(["w-3 h-3"], name: group.GroupIcon());
                                v.Text([], input.Category.ShortName());
                            }));
                    });
                    view.Text(["text-xs text-muted-foreground"],
                        $"Submitted {app.SubmittedAt:yyyy-MM-dd HH:mm} UTC · {input.Category.DisplayName()}");
                });

                // Routing verdict
                RenderRoutingBanner(view, result.Routing);

                // Score row
                view.Row(["gap-3"], content: view =>
                {
                    ScoreCard(view, "Completeness", result.CompletenessScore, isRisk: false);
                    ScoreCard(view, "Risk", result.RiskScore, isRisk: true);
                });

                // ── Applicant card ────────────────────────────────────────
                view.Column([Card.Default, "p-4 gap-0"], content: view =>
                {
                    view.Row(["items-center gap-2 mb-3"], content: view =>
                    {
                        view.Box(["w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center shrink-0"],
                            content: v => v.Icon(["text-primary w-4 h-4"], name: "user"));
                        view.Column(["gap-0"], content: view =>
                        {
                            view.Text(["font-semibold text-sm"], input.FullName);
                            view.Text(["text-xs text-muted-foreground"], input.Nationality);
                        });
                    });

                    DetailRow(view, "Email", input.Email);

                    if (input.PassportExpiry.HasValue)
                    {
                        var days = (input.PassportExpiry.Value - DateTime.UtcNow).Days;
                        var c    = days < 90 ? "text-error-primary" : days < 180 ? "text-warning-primary" : "text-success-primary";
                        view.Row(["justify-between py-1.5 border-b border-border/50"], content: view =>
                        {
                            view.Text(["text-xs text-muted-foreground"], "Passport Expiry");
                            view.Row(["items-center gap-1"], content: view =>
                            {
                                if (days < 180)
                                    view.Icon([$"{c} w-3 h-3"], name: days < 90 ? "alert-circle" : "alert-triangle");
                                view.Text([$"text-xs font-medium {c}"],
                                    $"{input.PassportExpiry.Value:yyyy-MM-dd} ({days}d)");
                            });
                        });
                    }
                });

                // ── Permit-type specific card ─────────────────────────────
                if (group == PermitReady.PermitGroup.Study)
                    RenderStudentCard(view, input);
                else if (group == PermitReady.PermitGroup.Work)
                    RenderWorkCard(view, input);
                else
                    RenderFamilyCard(view, input);

                // ── Immigration Process Pipeline ───────────────────────────
                RenderProcessPipeline(view, app);

                // ── Documents ────────────────────────────────────────────
                view.Column([Card.Default, "p-4 gap-2"], content: view =>
                {
                    view.Row(["items-center gap-1.5 mb-1"], content: view =>
                    {
                        view.Icon(["text-muted-foreground w-3.5 h-3.5"], name: "paperclip");
                        view.Text(["text-xs font-semibold text-muted-foreground uppercase tracking-wide"],
                            "Documents");
                        view.Text(["text-xs text-muted-foreground ml-auto"], "Click to review");
                    });
                    if (input.UploadedDocuments.Count == 0)
                    {
                        view.Text(["text-xs text-error-primary italic"], "No documents uploaded");
                    }
                    else
                    {
                        foreach (var doc in input.UploadedDocuments)
                        {
                            var docName = doc; // capture
                            view.Button([Button.GhostSm, "items-center gap-2 justify-start h-auto py-1.5 w-full"],
                                content: v =>
                                {
                                    v.Icon(["text-success-primary w-3 h-3 shrink-0"], name: "file-text");
                                    v.Text(["text-xs font-mono text-muted-foreground truncate"], docName);
                                    v.Text(["text-xs text-primary ml-auto shrink-0"], "View →");
                                },
                                onClick: async () =>
                                {
                                    _viewingDocAppId.Value = app.ApplicationId;
                                    _viewingDocFile.Value  = docName;
                                    _docViewerOpen.Value   = true;
                                });
                        }
                    }
                });

                // ── Issues ───────────────────────────────────────────────
                if (result.MissingItems.Count > 0)
                {
                    view.Column([Alert.Warning, "px-4 py-3 rounded-lg border gap-1.5"], content: view =>
                    {
                        view.Row(["items-center gap-1.5"], content: view =>
                        {
                            view.Icon(["text-warning-primary w-3.5 h-3.5"], name: "alert-triangle");
                            view.Text(["text-xs font-semibold text-warning-primary"],
                                $"{result.MissingItems.Count} Missing Item{(result.MissingItems.Count > 1 ? "s" : "")}");
                        });
                        foreach (var item in result.MissingItems)
                            view.Text(["text-xs text-warning-primary ml-5"], $"· {item}");
                    });
                }

                if (result.RiskFlags.Count > 0)
                {
                    view.Column([Alert.Danger, "px-4 py-3 rounded-lg border gap-1.5"], content: view =>
                    {
                        view.Row(["items-center gap-1.5"], content: view =>
                        {
                            view.Icon(["text-error-primary w-3.5 h-3.5"], name: "shield-alert");
                            view.Text(["text-xs font-semibold text-error-primary"],
                                $"{result.RiskFlags.Count} Risk Flag{(result.RiskFlags.Count > 1 ? "s" : "")}");
                        });
                        foreach (var flag in result.RiskFlags)
                            view.Text(["text-xs text-error-primary ml-5"], $"⚠ {flag}");
                    });
                }

                if (result.MissingItems.Count == 0 && result.RiskFlags.Count == 0)
                {
                    view.Row([Alert.Success, "px-4 py-3 rounded-lg border items-center gap-2"], content: view =>
                    {
                        view.Icon(["text-success-primary w-4 h-4 shrink-0"], name: "check-circle");
                        view.Text(["text-xs font-medium text-success-primary"],
                            "No issues — application is complete");
                    });
                }

                // ── Officer decision actions ──────────────────────────────
                RenderOfficerDecisionButton(view, app);

                // Auto-draft email block (if risk > 40)
                if (result.RiskScore > 40)
                    RenderSupplementEmailBlock(view, app.ApplicationId, input, result, isOfficer: true);
            });
        });
    }

    private static void RenderStudentCard(UIView view, PermitReady.ApplicationInput input)
    {
        view.Column([Card.Default, "p-4 gap-0"], content: view =>
        {
            view.Row(["items-center gap-1.5 mb-2"], content: view =>
            {
                view.Icon(["text-primary w-3.5 h-3.5"], name: "graduation-cap");
                view.Text(["text-xs font-semibold text-primary uppercase tracking-wide"], "Study Details");
            });
            if (!string.IsNullOrEmpty(input.UniversityName))
                DetailRow(view, "Institution", input.UniversityName!);
            if (!string.IsNullOrEmpty(input.ProgramName))
                DetailRow(view, "Programme", input.ProgramName!);
            if (input.FundsAmount.HasValue)
            {
                var fundsOk = input.FundsAmount.Value >= 560;
                view.Row(["justify-between py-1.5 border-b border-border/50 last:border-0"], content: view =>
                {
                    view.Text(["text-xs text-muted-foreground"], "Monthly Funds");
                    view.Row(["items-center gap-1"], content: view =>
                    {
                        view.Icon([$"{(fundsOk ? "text-success-primary" : "text-error-primary")} w-3 h-3"],
                            name: fundsOk ? "check-circle" : "alert-circle");
                        view.Text([$"text-xs font-semibold {(fundsOk ? "text-success-primary" : "text-error-primary")}"],
                            $"€{input.FundsAmount:N0}/mo");
                    });
                });
            }
        });
    }

    private static void RenderWorkCard(UIView view, PermitReady.ApplicationInput input)
    {
        view.Column([Card.Default, "p-4 gap-0"], content: view =>
        {
            view.Row(["items-center gap-1.5 mb-2"], content: view =>
            {
                view.Icon(["text-primary w-3.5 h-3.5"], name: "briefcase");
                view.Text(["text-xs font-semibold text-primary uppercase tracking-wide"], "Employment Details");
            });
            if (!string.IsNullOrEmpty(input.EmployerName))
                DetailRow(view, "Employer", input.EmployerName!);
            if (!string.IsNullOrEmpty(input.JobTitle))
                DetailRow(view, "Job Title", input.JobTitle!);
            if (!string.IsNullOrEmpty(input.EmploymentContractRef))
                DetailRow(view, "Contract Ref", input.EmploymentContractRef!);
            if (input.SalaryAmount.HasValue)
            {
                var salaryOk = input.SalaryAmount.Value >= 1500;
                view.Row(["justify-between py-1.5 border-b border-border/50 last:border-0"], content: view =>
                {
                    view.Text(["text-xs text-muted-foreground"], "Monthly Salary");
                    view.Row(["items-center gap-1"], content: view =>
                    {
                        view.Icon([$"{(salaryOk ? "text-success-primary" : "text-error-primary")} w-3 h-3"],
                            name: salaryOk ? "check-circle" : "alert-circle");
                        view.Text([$"text-xs font-semibold {(salaryOk ? "text-success-primary" : "text-error-primary")}"],
                            $"€{input.SalaryAmount:N0}/mo gross");
                    });
                });
            }
        });
    }

    private static void RenderFamilyCard(UIView view, PermitReady.ApplicationInput input)
    {
        view.Column([Card.Default, "p-4 gap-0"], content: view =>
        {
            view.Row(["items-center gap-1.5 mb-2"], content: view =>
            {
                view.Icon(["text-primary w-3.5 h-3.5"], name: "heart");
                view.Text(["text-xs font-semibold text-primary uppercase tracking-wide"], "Family / Sponsor Details");
            });
            if (!string.IsNullOrEmpty(input.SponsorName))
                DetailRow(view, "Sponsor", input.SponsorName!);
            if (!string.IsNullOrEmpty(input.SponsorPermitNumber))
                DetailRow(view, "Sponsor Permit", input.SponsorPermitNumber!);
            if (string.IsNullOrEmpty(input.SponsorName))
            {
                view.Row(["items-center gap-1 py-1.5"], content: view =>
                {
                    view.Icon(["text-error-primary w-3 h-3"], name: "alert-circle");
                    view.Text(["text-xs text-error-primary"], "No sponsor information provided");
                });
            }
        });
    }

    // ── Officer Decision Button ───────────────────────────────────────────

    private void RenderOfficerDecisionButton(UIView view, PermitReady.StoredApplication app)
    {
        // Don't show if already past decision stage
        bool canDecide = app.Status is PermitReady.ApplicationStatus.Screened
                                    or PermitReady.ApplicationStatus.UnderReview
                                    or PermitReady.ApplicationStatus.SupplementReceived;
        if (!canDecide) return;

        view.Column([Card.Default, "p-4 gap-3"], content: view =>
        {
            view.Row(["items-center gap-2"], content: view =>
            {
                view.Icon(["text-primary w-3.5 h-3.5"], name: "gavel");
                view.Text(["text-xs font-semibold text-primary uppercase tracking-wide"],
                    "Officer Decision");
                view.Box([Badge.DefaultSm, "ml-auto text-xs"], content: v => v.Text([], "Action Required"));
            });

            view.Text(["text-xs text-muted-foreground"],
                "Review the application and AI analysis, then submit your decision to proceed.");

            view.Row(["gap-2"], content: view =>
            {
                view.Button([Button.PrimaryMd, "flex-1 gap-2"],
                    content: v =>
                    {
                        v.Icon(["w-4 h-4"], name: "gavel");
                        v.Text([], "Make Decision");
                    },
                    onClick: async () =>
                    {
                        _pendingDecision.Value    = "";
                        _decisionNote.Value       = "";
                        _decisionSubmitting.Value = false;
                        _showDecisionPanel.Value  = true;
                    });
            });
        });
    }

    // ── RIGHT PANEL: AI Chat Interface ───────────────────────────────────

    private void RenderChatPanel(UIView view, PermitReady.StoredApplication app)
    {
        view.Column(["flex-1 h-full flex flex-col overflow-hidden bg-muted/20"], content: view =>
        {
            // Panel header
            view.Row(["px-5 py-3 border-b border-border bg-background items-center justify-between shrink-0"],
                content: view =>
                {
                    view.Row(["items-center gap-2"], content: view =>
                    {
                        view.Box(["w-7 h-7 rounded-lg bg-primary/10 flex items-center justify-center"],
                            content: v => v.Icon(["text-primary w-3.5 h-3.5"], name: "bot"));
                        view.Column(["gap-0"], content: view =>
                        {
                            view.Text(["text-sm font-semibold"], "AI Analysis Assistant");
                            view.Text(["text-xs text-muted-foreground"],
                                $"Analysing {app.Input.Category.ShortName()} · {app.ApplicationId}");
                        });
                    });

                    // Header buttons
                    view.Row(["items-center gap-2"], content: view =>
                    {
                        // Guidelines button
                        view.Button([Button.OutlineSm, "text-xs gap-1"],
                            content: v =>
                            {
                                v.Icon(["w-3.5 h-3.5"], name: "book-open");
                                v.Text([], "Guidelines");
                            },
                            disabled: _sourcesFetching.Value,
                            onClick: async () => { await FetchGuidelinesForCategoryAsync(app.Input.Category); });

                        // Audit log button
                        view.Button([Button.GhostSm, "text-xs gap-1"],
                            content: v =>
                            {
                                v.Icon(["w-3.5 h-3.5"], name: "history");
                                v.Text([], "History");
                            },
                            onClick: async () => { _showAuditLog.Value = true; });

                        // Restart chat button
                        view.Button([Button.GhostSm, "text-xs gap-1"],
                            content: v =>
                            {
                                v.Icon(["w-3.5 h-3.5"], name: "refresh-cw");
                                v.Text([], "Reset");
                            },
                            onClick: async () =>
                            {
                                _assessmentCache.Remove(app.ApplicationId);
                                _sourcesCache.Remove(app.ApplicationId);
                                await InitializeChatAsync(app, forceRefresh: true);
                            });
                    });
                });

            // Context Sources section
            RenderSourcesPanel(view, app);

            // Messages area (fills remaining space)
            view.ScrollArea(
                rootStyle: ["flex-1 min-h-0"],
                autoScroll: true,
                autoScrollKey: _chatMessages.Value.Count.ToString(),
                content: view =>
                {
                    view.Column(["px-5 py-4 gap-3 min-h-full"], content: view =>
                    {
                        if (_chatMessages.Value.Count == 0 && _chatStreaming.Value)
                        {
                            // Initial loading state
                            RenderThinkingBubble(view, "Analysing application...");
                        }

                        foreach (var msg in _chatMessages.Value)
                            RenderMessage(view, msg);

                        // Streaming indicator after last message
                        if (_chatStreaming.Value && _chatMessages.Value.Count > 0)
                            RenderThinkingBubble(view, "Thinking...");
                    });
                });

            // Input area
            RenderChatInput(view, app);
        });
    }

    // ── Sources panel ────────────────────────────────────────────────────

    private void RenderSourcesPanel(UIView view, PermitReady.StoredApplication app)
    {
        view.Column(["shrink-0 border-b border-border bg-background px-5 py-3 gap-2"], content: view =>
        {
            // Header row: source chips + add button
            view.Row(["items-center gap-2 flex-wrap"], content: view =>
            {
                view.Text(["text-xs font-medium text-muted-foreground shrink-0"], "Context:");

                // Always-present pre-screening chip
                view.Box(["flex items-center gap-1 bg-primary/10 border border-primary/20 rounded-full px-2.5 py-1"],
                    content: v =>
                    {
                        v.Icon(["text-primary w-3 h-3"], name: "clipboard-list");
                        v.Text(["text-xs font-medium text-primary"], "Pre-screening");
                    });

                // Added sources chips
                for (int i = 1; i < _chatSources.Value.Count; i++)
                {
                    var src = _chatSources.Value[i];
                    var idx = i; // capture for closure
                    var (icon, color) = src.Type switch
                    {
                        PermitReady.SourceType.Url  => ("link",        "text-blue-600"),
                        PermitReady.SourceType.File => ("file-text",   "text-emerald-600"),
                        _                           => ("file-plus",   "text-violet-600"),
                    };
                    view.Row(["items-center gap-1 bg-muted border border-border/60 rounded-full px-2.5 py-1 group"],
                        content: view =>
                        {
                            view.Icon([$"{color} w-3 h-3"], name: icon);
                            view.Text(["text-xs font-medium text-foreground max-w-[120px] truncate"],
                                src.Title);
                            view.Button([Button.GhostSm, "w-4 h-4 p-0 rounded-full opacity-60 hover:opacity-100"],
                                content: v => v.Icon(["w-2.5 h-2.5"], name: "x"),
                                onClick: async () =>
                                {
                                    var list = _chatSources.Value.ToList();
                                    list.RemoveAt(idx);
                                    _chatSources.Value = list;
                                });
                        });
                }

                // Add source button
                view.Button(
                    _showAddSource.Value
                        ? [Button.PrimarySm, "gap-1 rounded-full"]
                        : [Button.OutlineSm, "gap-1 rounded-full"],
                    content: v =>
                    {
                        v.Icon(["w-3 h-3"], name: _showAddSource.Value ? "x" : "plus");
                        v.Text([], _showAddSource.Value ? "Cancel" : "Add Source");
                    },
                    onClick: async () =>
                    {
                        _showAddSource.Value  = !_showAddSource.Value;
                        _sourceError.Value   = "";
                        _addSourceText.Value  = "";
                        _addSourceTitle.Value = "";
                    });
            });

            // Add source form (expandable)
            if (_showAddSource.Value)
                RenderAddSourceForm(view, app);

            if (!string.IsNullOrEmpty(_sourceError.Value))
                view.Text(["text-xs text-error-primary"], _sourceError.Value);
        });
    }

    private void RenderAddSourceForm(UIView view, PermitReady.StoredApplication app)
    {
        view.Column(["gap-2 pt-1"], content: view =>
        {
            // Type selector tabs
            view.Row(["gap-1"], content: view =>
            {
                foreach (var (type, icon, label) in new[] {
                    ("url",  "link",     "URL"),
                    ("text", "type",     "Text"),
                    ("file", "file-up",  "File"),
                })
                {
                    var t = type; // capture
                    view.Button(
                        _addSourceType.Value == t
                            ? [Button.PrimarySm, "gap-1"]
                            : [Button.GhostSm, "gap-1 text-muted-foreground"],
                        content: v => { v.Icon(["w-3 h-3"], name: icon); v.Text([], label); },
                        onClick: async () =>
                        {
                            _addSourceType.Value  = t;
                            _sourceError.Value    = "";
                            _addSourceText.Value  = "";
                        });
                }
            });

            if (_addSourceType.Value == "url")
            {
                view.Row(["gap-2 items-center"], content: view =>
                {
                    view.TextField([Input.DefaultSm, "flex-1"],
                        placeholder: "https://migri.fi/en/residence-permit-student",
                        value: _addSourceText.Value,
                        onValueChange: async v => { _addSourceText.Value = v; },
                        onSubmit: async _ => { await AddUrlSourceAsync(); });
                    view.Button(
                        _sourcesFetching.Value
                            ? [Button.PrimarySm, "gap-1 opacity-75"]
                            : [Button.PrimarySm, "gap-1"],
                        disabled: _sourcesFetching.Value,
                        content: v =>
                        {
                            if (_sourcesFetching.Value)
                                v.Box([Icon.Spinner, "w-3 h-3"]);
                            else
                                v.Icon(["w-3 h-3"], name: "download");
                            v.Text([], _sourcesFetching.Value ? "Fetching..." : "Fetch");
                        },
                        onClick: async () => { await AddUrlSourceAsync(); });
                });
                view.Text(["text-xs text-muted-foreground"],
                    "Paste a guideline URL (Migri, Finlex, institution website, etc.)");
            }
            else if (_addSourceType.Value == "text")
            {
                view.TextField([Input.DefaultSm],
                    placeholder: "Title (optional)",
                    value: _addSourceTitle.Value,
                    onValueChange: async v => { _addSourceTitle.Value = v; });
                view.TextArea([Input.Default, "min-h-[80px] text-xs resize-none"],
                    placeholder: "Paste guidelines, policy notes, or any relevant context...",
                    value: _addSourceText.Value,
                    onValueChange: async v => { _addSourceText.Value = v; });
                view.Button([Button.PrimarySm, "w-fit gap-1"],
                    content: v => { v.Icon(["w-3 h-3"], name: "plus"); v.Text([], "Add Note"); },
                    onClick: async () => { AddTextSource(); });
            }
            else // file
            {
                view.TextField([Input.DefaultSm],
                    placeholder: "Document label (optional)",
                    value: _addSourceTitle.Value,
                    onValueChange: async v => { _addSourceTitle.Value = v; });
                view.FileUploadZone(
                    accept: [".pdf"],
                    maxFileSize: 2 * 1024 * 1024,
                    onUploadComplete: async args => { await AddFileSourceAsync(args); },
                    onDragActiveChange: async d => { _dragSourceFile.Value = d; },
                    zoneStyle: _dragSourceFile.Value
                        ? [FileUpload.Zone.Compact, FileUpload.Zone.Active]
                        : [FileUpload.Zone.Compact],
                    activeStyle: [FileUpload.Zone.Active],
                    content: v =>
                    {
                        if (_sourcesFetching.Value)
                        {
                            v.Box([Icon.Spinner, "w-4 h-4 mb-1"]);
                            v.Text(["text-xs"], "Reading file...");
                        }
                        else
                        {
                            v.Icon([FileUpload.Icon.Base, "mb-1 w-5 h-5"], name: "upload");
                            v.Text(["text-xs font-medium"], "Drop PDF guideline here");
                            v.Text(["text-xs text-muted-foreground"], "Max 2 MB");
                        }
                    });
            }
        });
    }

    // ── Chat messages ─────────────────────────────────────────────────────

    private static void RenderMessage(UIView view, PermitReady.ChatMessage msg)
    {
        bool isUser = msg.Role == PermitReady.ChatRole.User;

        view.Row([isUser ? "justify-end" : "justify-start", "gap-2 items-end"], content: view =>
        {
            if (!isUser)
            {
                view.Box(["w-7 h-7 rounded-full bg-primary flex items-center justify-center shrink-0 mb-0.5"],
                    content: v => v.Icon(["text-primary-foreground w-3.5 h-3.5"], name: "bot"));
            }

            view.Column([
                isUser
                    ? "bg-primary text-primary-foreground rounded-2xl rounded-br-sm px-4 py-2.5 max-w-[75%]"
                    : "bg-background border border-border rounded-2xl rounded-bl-sm px-4 py-3 max-w-[85%] shadow-sm",
                "gap-1"
            ], content: view =>
            {
                if (isUser)
                    view.Text(["text-sm leading-relaxed"], msg.Content);
                else
                    view.Markdown(["text-sm leading-relaxed prose prose-sm max-w-none dark:prose-invert"],
                        content: msg.Content);

                view.Text([
                    isUser ? "text-primary-foreground/60" : "text-muted-foreground",
                    "text-[10px]"
                ], msg.Timestamp.ToString("HH:mm"));
            });

            if (isUser)
            {
                view.Box(["w-7 h-7 rounded-full bg-muted flex items-center justify-center shrink-0 mb-0.5"],
                    content: v => v.Icon(["text-muted-foreground w-3.5 h-3.5"], name: "user"));
            }
        });
    }

    private static void RenderThinkingBubble(UIView view, string label)
    {
        view.Row(["justify-start items-end gap-2"], content: view =>
        {
            view.Box(["w-7 h-7 rounded-full bg-primary flex items-center justify-center shrink-0"],
                content: v => v.Icon(["text-primary-foreground w-3.5 h-3.5"], name: "bot"));
            view.Box(["bg-background border border-border rounded-2xl rounded-bl-sm px-4 py-3 shadow-sm"],
                content: v =>
                {
                    v.Row(["items-center gap-2"], content: v =>
                    {
                        v.Box([Icon.Spinner, "w-3.5 h-3.5 text-primary"]);
                        v.Text(["text-sm text-muted-foreground"], label);
                    });
                });
        });
    }

    // ── Chat input ────────────────────────────────────────────────────────

    private void RenderChatInput(UIView view, PermitReady.StoredApplication app)
    {
        view.Column(["px-5 py-4 border-t border-border bg-background shrink-0 gap-3"], content: view =>
        {
            // Suggested quick queries (shown only before first user message)
            bool hasUserMsg = _chatMessages.Value.Any(m => m.Role == PermitReady.ChatRole.User);
            if (!hasUserMsg && !_chatStreaming.Value && _chatMessages.Value.Count > 0)
            {
                var group = app.Input.Group;
                var queries = group == PermitReady.PermitGroup.Study
                    ? new[]
                    {
                        "What documents are missing?",
                        "Are the funds sufficient for this programme?",
                        "What are the passport risks?",
                        "What follow-up questions should I ask?"
                    }
                    : group == PermitReady.PermitGroup.Work
                    ? new[]
                    {
                        "Is the salary compliant with Finnish law?",
                        "What are the main risk flags?",
                        "What documents should I request?",
                        "Summarise the employer credibility risk"
                    }
                    : new[]
                    {
                        "Is the sponsor relationship adequately documented?",
                        "What family documents are missing?",
                        "What are the key eligibility requirements?",
                        "Are there any red flags in this application?"
                    };

                view.Row(["flex-wrap gap-2"], content: view =>
                {
                    foreach (var q in queries)
                    {
                        var query = q; // capture
                        view.Button([Button.OutlineSm, "text-xs rounded-full h-auto py-1.5 px-3 text-left"],
                            label: q,
                            onClick: async () =>
                            {
                                _chatInput.Value = query;
                                await SendChatMessageAsync(app);
                            });
                    }
                });
            }

            // Input row
            view.Row(["gap-2 items-end"], content: view =>
            {
                view.TextArea([Input.Default, "flex-1 resize-none min-h-[44px] max-h-[120px] text-sm py-2.5"],
                    placeholder: "Ask about this application… (Enter to send, Shift+Enter for new line)",
                    value: _chatInput.Value,
                    onValueChange: async v => { _chatInput.Value = v; },
                    onSubmit: async _ => { await SendChatMessageAsync(app); });

                view.Button(
                    _chatStreaming.Value
                        ? [Button.PrimaryMd, Button.Size.Icon, "shrink-0 opacity-60"]
                        : [Button.PrimaryMd, Button.Size.Icon, "shrink-0"],
                    disabled: _chatStreaming.Value || string.IsNullOrWhiteSpace(_chatInput.Value),
                    onClick: async () => { await SendChatMessageAsync(app); },
                    content: v =>
                    {
                        if (_chatStreaming.Value)
                            v.Box([Icon.Spinner, "w-4 h-4"]);
                        else
                            v.Icon(["w-4 h-4"], name: "send");
                    });
            });
        });
    }

    private static void DetailRow(UIView view, string label, string value)
    {
        view.Row(["justify-between py-1.5 border-b border-border/50 last:border-0 gap-2"], content: view =>
        {
            view.Text(["text-xs text-muted-foreground shrink-0"], label);
            view.Text(["text-xs font-medium text-right"], value);
        });
    }
}
