return await App.Run(args);

public record SessionIdentity(string UserId);
public record ClientParameters(string Name = "PermitReady");

[App]
public partial class IkonDemoApp(IApp<SessionIdentity, ClientParameters> app)
{
    private readonly IAppBase _host = app;   // backing field so DB helpers can access it across partial files
    private UI    UI    { get; } = new(app, new Theme());
    private Audio Audio { get; } = new(app);

    // ── Shared state (all connected clients see the same) ─────────────────
    private readonly Reactive<List<PermitReady.StoredApplication>> _applications = new([]);

    // ── Per-client state (each browser tab has its own copy) ──────────────
    private readonly ClientReactive<string>  _page      = new("landing");   // landing | form | results | dashboard | detail
    private readonly ClientReactive<string>  _role      = new("");           // "" | "applicant" | "officer"
    private readonly ClientReactive<string>  _theme     = new(Constants.LightTheme);

    // Form state — permit category
    private readonly ClientReactive<string>  _permitCategory    = new("StudentHigherEd");
    private readonly ClientReactive<string>  _permitGroup       = new("Study");

    // Form state — personal
    private readonly ClientReactive<string>  _fullName       = new("");
    private readonly ClientReactive<string>  _nationality    = new("");
    private readonly ClientReactive<string>  _email          = new("");
    private readonly ClientReactive<string>  _phone          = new("");
    private readonly ClientReactive<string>  _passportNumber = new("");
    private readonly ClientReactive<string>  _passportExpiry = new("");
    // Form state — study
    private readonly ClientReactive<string>  _universityName = new("");
    private readonly ClientReactive<string>  _programName    = new("");
    private readonly ClientReactive<string>  _fundsAmount    = new("");
    private readonly ClientReactive<string>  _studyStartDate = new("");
    // Form state — work
    private readonly ClientReactive<string>  _employerName   = new("");
    private readonly ClientReactive<string>  _jobTitle       = new("");
    private readonly ClientReactive<string>  _salaryAmount   = new("");
    private readonly ClientReactive<string>  _contractRef    = new("");
    private readonly ClientReactive<string>  _workStartDate  = new("");
    // Form state — family
    private readonly ClientReactive<string>  _sponsorName          = new("");
    private readonly ClientReactive<string>  _sponsorPermitNumber  = new("");
    private readonly ClientReactive<string>  _relationshipType     = new("");  // spouse, child, parent
    private readonly ClientReactive<string>  _sponsorNationality   = new("");
    private readonly ClientReactive<string>  _sponsorOccupation    = new("");
    // Form state — work (entrepreneur)
    private readonly ClientReactive<string>  _businessName         = new("");
    private readonly ClientReactive<string>  _businessRegistration = new("");
    private readonly ClientReactive<string>  _businessField        = new("");
    private readonly ClientReactive<string>  _businessFunds        = new("");
    // Form state — work (specialist)
    private readonly ClientReactive<string>  _fieldOfExpertise     = new("");
    private readonly ClientReactive<string>  _yearsOfExperience    = new("");
    private readonly ClientReactive<string>  _certifications       = new("");
    // Form state — work (researcher)
    private readonly ClientReactive<string>  _researchInstitution  = new("");
    private readonly ClientReactive<string>  _researchProject      = new("");
    // Form state — study
    private readonly ClientReactive<string>  _exchangeOrganization = new("");
    private readonly ClientReactive<string>  _scholarshipSource    = new("");
    // Upload slots — multiple documents supported per slot
    private readonly ClientReactive<List<PermitReady.UploadedDoc>> _passportDoc      = new([]);
    private readonly ClientReactive<List<PermitReady.UploadedDoc>> _acceptanceDoc    = new([]);
    private readonly ClientReactive<List<PermitReady.UploadedDoc>> _transcriptDoc    = new([]);
    private readonly ClientReactive<List<PermitReady.UploadedDoc>> _bankStatementDoc = new([]);
    private readonly ClientReactive<List<PermitReady.UploadedDoc>> _contractDoc      = new([]);
    private readonly ClientReactive<List<PermitReady.UploadedDoc>> _salaryProofDoc   = new([]);
    // Submission
    private readonly ClientReactive<bool>    _isSubmitting   = new(false);
    private readonly ClientReactive<string>  _formError      = new("");

    // Results / detail state
    private readonly ClientReactive<PermitReady.ScreeningResult?> _lastResult   = new(null);
    private readonly ClientReactive<PermitReady.ApplicationInput?> _lastInput    = new(null);
    private readonly ClientReactive<string?>                       _selectedAppId = new(null);

    // Officer PIN entry
    private readonly ClientReactive<string>  _officerPin     = new("");
    private readonly ClientReactive<string>  _pinError       = new("");

    // Dashboard tab & filters
    private readonly ClientReactive<string>  _dashboardTab      = new("all");
    private readonly ClientReactive<string>  _filterCategory    = new("");          // empty = all
    private readonly ClientReactive<string>  _filterStatus      = new("");          // empty = all
    private readonly ClientReactive<string>  _filterRiskLevel   = new("");          // empty | "low" | "medium" | "high"
    private readonly ClientReactive<string>  _filterCompleteLevel = new("");        // empty | "low" | "medium" | "high"
    private readonly ClientReactive<bool>    _showFilters       = new(false);

    // Application tracking (applicant home page)
    private readonly ClientReactive<string>  _trackId        = new("");
    private readonly ClientReactive<string>  _trackError     = new("");
    private readonly ClientReactive<PermitReady.StoredApplication?> _trackedApp = new(null);

    // Last submitted application ID (for results page)
    private readonly ClientReactive<string>  _lastAppId      = new("");

    // ── Officer AI chat state (per-client) ───────────────────────────────
    private readonly ClientReactive<List<PermitReady.ChatMessage>>    _chatMessages  = new([]);
    private readonly ClientReactive<List<PermitReady.AnalysisSource>> _chatSources   = new([]);
    private readonly ClientReactive<string> _chatInput       = new("");
    private readonly ClientReactive<bool>   _chatStreaming    = new(false);
    private readonly ClientReactive<string> _addSourceType   = new("text");  // "text" | "url" | "file"
    private readonly ClientReactive<string> _addSourceText   = new("");
    private readonly ClientReactive<string> _addSourceTitle  = new("");
    private readonly ClientReactive<bool>   _showAddSource   = new(false);
    private readonly ClientReactive<bool>   _dragSourceFile  = new(false);
    private readonly ClientReactive<string> _sourceError     = new("");
    private readonly ClientReactive<bool>   _sourcesFetching = new(false);
    private KernelContext _chatContext = new();

    // ── Chat optimisation: per-application cache ──────────────────────────
    private readonly Dictionary<string, string> _assessmentCache = new();
    private readonly Dictionary<string, List<PermitReady.AnalysisSource>> _sourcesCache = new();

    // ── Chat audit log (shared across all officer sessions) ───────────────
    private readonly Reactive<List<PermitReady.ChatAuditEntry>> _chatAuditLog = new([]);

    // ── Official messages (Officer ↔ Applicant — shared, real-time) ──────
    private readonly Reactive<List<PermitReady.OfficialMessage>>  _officialMessages = new([]);
    private readonly Reactive<List<PermitReady.ProfileAccessEntry>> _profileAccessLog = new([]);

    // ── Officer decision state (per-client) ───────────────────────────────
    private readonly ClientReactive<bool>   _showDecisionPanel  = new(false);
    private readonly ClientReactive<string> _pendingDecision    = new("");   // "approve"|"supplement"|"reject"
    private readonly ClientReactive<string> _decisionNote       = new("");
    private readonly ClientReactive<bool>   _decisionSubmitting = new(false);

    // ── Audit log panel (per-client) ──────────────────────────────────────
    private readonly ClientReactive<bool>   _showAuditLog       = new(false);

    // ── Profile access reason dialog (per-client — GDPR Article 5) ───────
    private readonly ClientReactive<bool>   _showAccessReasonDialog = new(false);
    private readonly ClientReactive<string> _pendingAccessAppId     = new("");
    private readonly ClientReactive<string> _accessReason           = new("");
    private readonly ClientReactive<string> _accessReasonError      = new("");

    // ── Message thread tab (per-client) ───────────────────────────────────
    private readonly ClientReactive<string> _messageThreadTab       = new("ai"); // "ai" | "messages"

    // ── Officer message compose (per-client) ──────────────────────────────
    private readonly ClientReactive<bool>   _showComposeMessage     = new(false);
    private readonly ClientReactive<string> _composeMessageType     = new("GeneralMessage");
    private readonly ClientReactive<string> _composeMessageBody     = new("");

    // ── Applicant reply compose (per-client) ──────────────────────────────
    private readonly ClientReactive<bool>   _showReplyCompose    = new(false);
    private readonly ClientReactive<string> _replyBody           = new("");
    private readonly ClientReactive<bool>   _dragReplyFile       = new(false);
    private readonly ClientReactive<string> _replyAttachFileName = new("");

    // ── AI draft suggestion (per-client) ─────────────────────────────────
    private readonly ClientReactive<bool>   _chatHasDraft       = new(false);
    private readonly ClientReactive<string> _chatDraftContent   = new("");
    private readonly ClientReactive<string> _chatDraftType      = new("none");

    // ── Dashboard guide — shown every time the dashboard is opened ────────
    private readonly ClientReactive<bool>   _dashboardGuideDismissed = new(false);

    // ── Payment state (per-client) ────────────────────────────────────────
    private readonly ClientReactive<string> _paymentMethod     = new("online-banking");
    private readonly ClientReactive<bool>   _paymentProcessing = new(false);

    // ── Applicant home panel state ────────────────────────────────────────
    // "info"     → campaign awareness (full left panel)
    // "applying" → narrow brand strip + inline form
    private readonly ClientReactive<string> _homePanel = new("info");

    // ── Document viewer state (per-client) ────────────────────────────────
    private readonly ClientReactive<bool>   _docViewerOpen    = new(false);
    private readonly ClientReactive<string> _viewingDocAppId  = new("");
    private readonly ClientReactive<string> _viewingDocFile   = new("");
    private readonly ClientReactive<string> _viewingDocDataUrl = new("");   // base64 data URL

    // ── Uploaded doc bytes cache (per-client, keyed by filename) ─────────
    private readonly ClientReactive<Dictionary<string, byte[]>> _uploadedDocBytes = new(new());

    // ── Lifecycle ─────────────────────────────────────────────────────────
    public async Task Main()
    {
        // Sync page state when browser navigates (back/forward button)
        app.Navigation.PathChangedAsync += async args =>
        {
            var page = PathToPage(args.Path);
            if (page != null) _page.Value = page;
        };

        app.ClientJoinedAsync += async args =>
        {
            var ctx = args.ClientContext;

            // Restore page from URL on initial load (e.g. direct link, refresh)
            var initialPage = PathToPage(ctx.InitialPath);
            if (initialPage != null) _page.Value = initialPage;

            if (!string.IsNullOrEmpty(ctx.Theme))
                _theme.Value = ctx.Theme == Constants.DarkTheme ? Constants.DarkTheme : Constants.LightTheme;
        };

        // Initialise database and load persisted applications (falls back to seed data if DB unavailable)
        try
        {
            await PermitReady.PermitReadyDb.InitializeAsync(app);
            var dbApps = await PermitReady.PermitReadyDb.LoadApplicationsAsync(app);
            if (dbApps.Count > 0)
            {
                _applications.Value = dbApps;
            }
            else
            {
                SeedDemoData();
                // Persist seed data to DB for next startup
                foreach (var a in _applications.Value)
                    _ = PermitReady.PermitReadyDb.SaveApplicationAsync(app, a);
            }

            // Load persisted official messages
            var dbMessages = await PermitReady.PermitReadyDb.LoadOfficialMessagesAsync(app);
            if (dbMessages.Count > 0)
                _officialMessages.Value = dbMessages;
        }
        catch
        {
            // DB not configured yet — use in-memory seed data
            SeedDemoData();
        }

        UI.Root([Page.Default], content: view =>
        {
            view.Column(["h-screen flex flex-col"], content: view =>
            {
                RenderHeader(view);

                view.Box(["flex-1 overflow-hidden"], content: view =>
                {
                    switch (_page.Value)
                    {
                        case "landing":        RenderLanding(view);        break;
                        case "applicant_home": RenderApplicantHome(view);  break;
                        case "assessment":     RenderPermitAssessment(view); break;
                        case "form":           RenderForm(view);            break;
                        case "review":         RenderReview(view);          break;
                        case "payment":        RenderPayment(view);         break;
                        case "results":        RenderResults(view);         break;
                        case "dashboard":      RenderDashboard(view);       break;
                        case "detail":         RenderDetail(view);          break;
                    }
                });

                // Profile access reason dialog (GDPR — must appear before detail view)
                RenderAccessReasonDialog(view);
                // Officer decision dialog (global overlay)
                RenderDecisionDialog(view);
                // Audit log dialog (global overlay)
                RenderAuditLogDialog(view);

                // Global document viewer dialog (renders as overlay above all pages)
                RenderDocViewerDialog(view);
            });
        });
    }

    // ── Navigation — updates both app state and browser URL ───────────────
    private void Navigate(string page)
    {
        if (page == "dashboard")      _dashboardGuideDismissed.Value = false;
        if (page == "applicant_home") _homePanel.Value = "info";   // always start on campaign view
        _page.Value = page;
        _ = ClientFunctions.SetUrlAsync(PageToPath(page));
    }

    // ── URL ↔ page mapping ────────────────────────────────────────────────
    private static string PageToPath(string page) => page switch
    {
        "landing"        => "/",
        "applicant_home" => "/home",
        "assessment"     => "/assess",
        "form"           => "/apply",
        "review"         => "/review",
        "payment"        => "/payment",
        "results"        => "/results",
        "dashboard"      => "/dashboard",
        "detail"         => "/detail",
        _                => "/"
    };

    private static string? PathToPage(string path) => path.TrimStart('/') switch
    {
        "" or "landing"  => "landing",
        "home"           => "applicant_home",
        "assess"         => "assessment",
        "apply"          => "form",
        "review"         => "review",
        "payment"        => "payment",
        "results"        => "results",
        "dashboard"      => "dashboard",
        "detail"         => "detail",
        _                => null
    };

    // ── Theme toggle ──────────────────────────────────────────────────────
    private async Task ToggleThemeAsync()
    {
        var next = _theme.Value == Constants.DarkTheme ? Constants.LightTheme : Constants.DarkTheme;
        var ok = await ClientFunctions.SetThemeAsync(next);
        if (ok) _theme.Value = next;
    }

    // ── Phase 1: Score and route to review (or directly to results if clean) ─
    private async Task SubmitApplicationAsync()
    {
        if (_isSubmitting.Value) return;

        _formError.Value = "";

        // Basic validation
        if (string.IsNullOrWhiteSpace(_fullName.Value) ||
            string.IsNullOrWhiteSpace(_nationality.Value) ||
            string.IsNullOrWhiteSpace(_email.Value))
        {
            _formError.Value = "Please fill in your full name, nationality, and email.";
            return;
        }

        _isSubmitting.Value = true;

        try
        {
            var category = Enum.TryParse<PermitReady.PermitCategory>(_permitCategory.Value, out var cat)
                ? cat
                : PermitReady.PermitCategory.StudentHigherEd;

            DateTime? passportExpiry = DateTime.TryParse(_passportExpiry.Value, out var dt) ? dt : null;
            decimal?  funds          = decimal.TryParse(_fundsAmount.Value,  out var f) ? f : null;
            decimal?  salary         = decimal.TryParse(_salaryAmount.Value, out var s) ? s : null;

            // Build docs list from individual upload slots (for scoring engine keyword matching)
            var docs = new List<string>();
            void AddDocs(List<PermitReady.UploadedDoc> list) { foreach (var d in list) if (d.Status != PermitReady.DocStatus.Failed) docs.Add(d.FileName); }
            AddDocs(_passportDoc.Value);
            AddDocs(_acceptanceDoc.Value);
            AddDocs(_transcriptDoc.Value);
            AddDocs(_bankStatementDoc.Value);
            AddDocs(_contractDoc.Value);
            AddDocs(_salaryProofDoc.Value);

            // Use AI-extracted expiry date as fallback if user didn't type one
            var firstPassport = _passportDoc.Value.FirstOrDefault();
            if (passportExpiry == null && firstPassport?.Extracted.TryGetValue("ExpiryDate", out var aiExpiry) == true)
                passportExpiry = DateTime.TryParse(aiExpiry, out var dt2) ? dt2 : null;

            // Collect AI-extracted fields from verified documents (use first doc per slot)
            var extractedDocFields = new Dictionary<string, string>();
            void Collect(List<PermitReady.UploadedDoc> list, string prefix)
            {
                var doc = list.FirstOrDefault();
                if (doc?.Extracted == null) return;
                foreach (var kv in doc.Extracted)
                    extractedDocFields[$"{prefix}_{kv.Key.ToLowerInvariant()}"] = kv.Value;
            }
            Collect(_passportDoc.Value,      "passport");
            Collect(_bankStatementDoc.Value, "bank");
            Collect(_contractDoc.Value,      "contract");
            Collect(_salaryProofDoc.Value,   "salary");

            var input = new PermitReady.ApplicationInput(
                FullName:              _fullName.Value.Trim(),
                Nationality:           _nationality.Value.Trim(),
                Email:                 _email.Value.Trim(),
                Category:              category,
                UniversityName:        _universityName.Value.NullIfEmpty(),
                ProgramName:           _programName.Value.NullIfEmpty(),
                FundsAmount:           funds,
                EmployerName:          _employerName.Value.NullIfEmpty(),
                JobTitle:              _jobTitle.Value.NullIfEmpty(),
                SalaryAmount:          salary,
                EmploymentContractRef: _contractRef.Value.NullIfEmpty(),
                SponsorName:           _sponsorName.Value.NullIfEmpty(),
                SponsorPermitNumber:   _sponsorPermitNumber.Value.NullIfEmpty(),
                UploadedDocuments:     docs,
                PassportExpiry:        passportExpiry,
                ExtractedDocFields:    extractedDocFields.Count > 0 ? extractedDocFields : null
            );

            var result = PermitReady.ScoringService.Score(input);

            // Store scored input for review/results pages
            _lastInput.Value  = input;
            _lastResult.Value = result;

            // If any issues exist → go to review page so applicant can fix before paying
            bool hasIssues = result.MissingItems.Count > 0 || result.RiskFlags.Count > 0;
            if (hasIssues)
            {
                Navigate("review");
            }
            else
            {
                // Perfect application — go straight to payment
                Navigate("payment");
            }
        }
        finally
        {
            _isSubmitting.Value = false;
        }
    }

    // ── Phase 2: Applicant confirmed issues — proceed to payment ─────────
    private void ConfirmSubmissionAsync()
    {
        var input  = _lastInput.Value;
        var result = _lastResult.Value;
        if (input == null || result == null) return;
        Navigate("payment");
    }

    // ── Phase 3: Simulate payment gateway, then commit to officer queue ───
    private async Task ProcessPaymentAsync()
    {
        if (_paymentProcessing.Value) return;
        var input  = _lastInput.Value;
        var result = _lastResult.Value;
        if (input == null || result == null) { Navigate("form"); return; }

        _paymentProcessing.Value = true;
        try
        {
            await Task.Delay(2200); // simulate bank/card gateway round-trip

            var fee    = GetApplicationFee(input.Category);
            var payRef = $"PRN-{DateTime.UtcNow:yyyyMMddHHmm}-{new Random().Next(10000, 99999)}";

            CommitApplication(input, result, fee, payRef);
            Navigate("results");
        }
        finally
        {
            _paymentProcessing.Value = false;
        }
    }

    // ── Fee schedule — Finnish Immigration Service 2026 (online rates) ────
    private static decimal GetApplicationFee(PermitReady.PermitCategory cat) => cat switch
    {
        PermitReady.PermitCategory.WorkingHoliday   => 100m,
        PermitReady.PermitCategory.SeasonalWorker   => 530m,
        PermitReady.PermitCategory.AuPair           => 530m,
        PermitReady.PermitCategory.LanguageCourse   => 300m,
        PermitReady.PermitCategory.Researcher       => 600m,
        PermitReady.PermitCategory.StudentHigherEd  => 600m,
        PermitReady.PermitCategory.StudentVocational => 600m,
        PermitReady.PermitCategory.ExchangeStudent  => 600m,
        PermitReady.PermitCategory.TraineeIntern    => 600m,
        _                                           => 750m,
    };

    // ── Internal: write to shared applications list ───────────────────────
    private void CommitApplication(
        PermitReady.ApplicationInput input,
        PermitReady.ScreeningResult result,
        decimal feeAmount = 0m,
        string? paymentReference = null)
    {
        var appId  = $"A{(_applications.Value.Count + 1):D3}";
        var stored = new PermitReady.StoredApplication(
            appId, input, result, DateTime.UtcNow,
            PaymentStatus:    PermitReady.PaymentStatus.Paid,
            FeeAmount:        feeAmount,
            PaymentReference: paymentReference);
        _applications.Value = [.. _applications.Value, stored];
        _lastAppId.Value    = appId;

        // Persist to DB (fire-and-forget) — also saves any uploaded doc bytes
        _ = PersistApplicationAsync(stored);
    }

    private async Task PersistApplicationAsync(PermitReady.StoredApplication stored)
    {
        await PermitReady.PermitReadyDb.SaveApplicationAsync(app, stored);

        // Save cached document bytes to DB
        foreach (var (filename, bytes) in _uploadedDocBytes.Value)
            await PermitReady.PermitReadyDb.SaveDocumentAsync(app, stored.ApplicationId, filename, bytes);

        _uploadedDocBytes.Value.Clear();
    }

    // ── Officer PIN verification ──────────────────────────────────────────
    private void VerifyOfficerPin()
    {
        if (_officerPin.Value == "1234")
        {
            _pinError.Value = "";
            _role.Value     = "officer";
            Navigate("dashboard");
        }
        else
        {
            _pinError.Value = "Incorrect PIN. Try 1234 for this demo.";
        }
    }

    // ── Update application status ─────────────────────────────────────────
    private void UpdateApplicationStatus(
        string appId,
        PermitReady.ApplicationStatus newStatus,
        string? officerNotes = null)
    {
        var apps = _applications.Value.ToList();
        var idx  = apps.FindIndex(a => a.ApplicationId == appId);
        if (idx < 0) return;
        var updated = apps[idx] with
        {
            Status          = newStatus,
            StatusChangedAt = DateTime.UtcNow,
            OfficerNotes    = officerNotes ?? apps[idx].OfficerNotes,
        };
        apps[idx] = updated;
        _applications.Value = apps;

        // Persist status change to DB (fire-and-forget)
        _ = PermitReady.PermitReadyDb.SaveApplicationAsync(app, updated);
    }

    // ── Officer submits a decision ────────────────────────────────────────
    private void SubmitOfficerDecision(string appId, string decision, string notes)
    {
        var newStatus = decision switch
        {
            "approve"    => PermitReady.ApplicationStatus.Approved,
            "supplement" => PermitReady.ApplicationStatus.SupplementRequested,
            _            => PermitReady.ApplicationStatus.Rejected,
        };

        UpdateApplicationStatus(appId, newStatus, string.IsNullOrWhiteSpace(notes) ? null : notes);

        // Auto-post a notification message into the official thread
        GenerateStatusNotificationMessage(appId, newStatus, string.IsNullOrWhiteSpace(notes) ? null : notes);

        _showDecisionPanel.Value  = false;
        _pendingDecision.Value    = "";
        _decisionNote.Value       = "";
        _decisionSubmitting.Value = false;

        // Auto-advance to AwaitingBiometrics when approved
        if (newStatus == PermitReady.ApplicationStatus.Approved)
            _ = AutoAdvanceAfterApprovalAsync(appId);
    }

    private async Task AutoAdvanceAfterApprovalAsync(string appId)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        UpdateApplicationStatus(appId, PermitReady.ApplicationStatus.AwaitingBiometrics);
    }

    // ── Simulate advancing status (officer triggers from UI) ─────────────
    private void AdvanceApplicationStatus(string appId)
    {
        var app = _applications.Value.FirstOrDefault(a => a.ApplicationId == appId);
        if (app == null) return;

        var next = app.Status switch
        {
            PermitReady.ApplicationStatus.AwaitingBiometrics  => PermitReady.ApplicationStatus.BiometricsComplete,
            PermitReady.ApplicationStatus.BiometricsComplete  => PermitReady.ApplicationStatus.PermitInProduction,
            PermitReady.ApplicationStatus.PermitInProduction  => PermitReady.ApplicationStatus.ReadyForCollection,
            PermitReady.ApplicationStatus.ReadyForCollection  => PermitReady.ApplicationStatus.PermitIssued,
            PermitReady.ApplicationStatus.SupplementRequested => PermitReady.ApplicationStatus.SupplementReceived,
            PermitReady.ApplicationStatus.SupplementReceived  => PermitReady.ApplicationStatus.UnderReview,
            _ => app.Status,
        };

        if (next != app.Status)
            UpdateApplicationStatus(appId, next);
    }

    // ── Application tracking ──────────────────────────────────────────────
    private void TrackApplication()
    {
        _trackError.Value  = "";
        _trackedApp.Value  = null;

        var id = _trackId.Value.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(id))
        {
            _trackError.Value = "Please enter your application reference number.";
            return;
        }

        var found = _applications.Value.FirstOrDefault(a =>
            a.ApplicationId.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (found == null)
        {
            _trackError.Value = $"No application found with ID \"{id}\". Check the reference number on your confirmation.";
            return;
        }

        _trackedApp.Value = found;
    }

    // ── Official messaging ────────────────────────────────────────────────

    private void PostOfficialMessage(
        string appId,
        PermitReady.OfficialMessageType type,
        string senderRole,
        string content,
        string? attachment = null)
    {
        var msgs = _officialMessages.Value.ToList();
        var seq  = msgs.Count(m => m.ApplicationId == appId) + 1;
        var msg  = new PermitReady.OfficialMessage(
            MessageId:          $"MSG-{appId}-{seq:D3}",
            ApplicationId:      appId,
            Type:               type,
            SenderRole:         senderRole,
            Content:            content,
            SentAt:             DateTime.UtcNow,
            ReadByOfficer:      senderRole is "officer" or "system",
            ReadByApplicant:    senderRole == "applicant",
            AttachmentFileName: attachment);
        msgs.Add(msg);
        _officialMessages.Value = msgs;
        _ = PermitReady.PermitReadyDb.SaveOfficialMessageAsync(_host, msg);
    }

    private void MarkThreadReadByOfficer(string appId)
    {
        _officialMessages.Value = _officialMessages.Value
            .Select(m => m.ApplicationId == appId && !m.ReadByOfficer
                ? m with { ReadByOfficer = true } : m)
            .ToList();
    }

    private void MarkThreadReadByApplicant(string appId)
    {
        _officialMessages.Value = _officialMessages.Value
            .Select(m => m.ApplicationId == appId && !m.ReadByApplicant
                ? m with { ReadByApplicant = true } : m)
            .ToList();
    }

    private int UnreadApplicantReplies(string appId) =>
        _officialMessages.Value.Count(m =>
            m.ApplicationId == appId &&
            m.SenderRole == "applicant" &&
            !m.ReadByOfficer);

    private int UnreadOfficerMessages(string appId) =>
        _officialMessages.Value.Count(m =>
            m.ApplicationId == appId &&
            m.SenderRole is "officer" or "system" &&
            m.Type != PermitReady.OfficialMessageType.ProfileAccessNotice &&
            !m.ReadByApplicant);

    private void GenerateStatusNotificationMessage(
        string appId,
        PermitReady.ApplicationStatus status,
        string? officerNotes)
    {
        string? content = status switch
        {
            PermitReady.ApplicationStatus.Approved =>
                "Your application has been approved. You will receive an invitation to book a biometrics appointment shortly." +
                (string.IsNullOrEmpty(officerNotes) ? "" : $"\n\nOfficer note: {officerNotes}"),

            PermitReady.ApplicationStatus.SupplementRequested =>
                "Additional information or documents are required before your application can be processed.\n\n" +
                (string.IsNullOrEmpty(officerNotes) ? "Please log in and check the requested items." : $"Required items: {officerNotes}") +
                "\n\nPlease reply to this message within 30 days to avoid your application being paused.",

            PermitReady.ApplicationStatus.Rejected =>
                "Following careful review, your application has not been approved at this time." +
                (string.IsNullOrEmpty(officerNotes) ? "" : $"\n\nReason: {officerNotes}") +
                "\n\nYou may appeal within 30 days or reapply with corrected documents. Contact Migri at info@migri.fi",

            _ => null
        };

        if (content == null) return;

        var msgType = status == PermitReady.ApplicationStatus.SupplementRequested
            ? PermitReady.OfficialMessageType.SupplementRequest
            : PermitReady.OfficialMessageType.StatusNotification;

        PostOfficialMessage(appId, msgType, "officer", content);
    }

    // ── Profile access: confirm reason and navigate ───────────────────────

    private async Task ConfirmProfileAccessAndNavigateAsync()
    {
        var reason = _accessReason.Value.Trim();
        if (string.IsNullOrEmpty(reason))
        {
            _accessReasonError.Value = "Please state your reason for accessing this file.";
            return;
        }

        var appId = _pendingAccessAppId.Value;

        // Log access (in-memory + DB)
        _profileAccessLog.Value = [.. _profileAccessLog.Value,
            new PermitReady.ProfileAccessEntry(appId, DateTime.UtcNow, reason)];
        _ = PermitReady.PermitReadyDb.SaveProfileAccessAsync(_host,
            new PermitReady.ProfileAccessEntry(appId, DateTime.UtcNow, reason));

        // Send profile access notice — applicant sees this in their message thread
        PostOfficialMessage(appId, PermitReady.OfficialMessageType.ProfileAccessNotice, "system",
            "A Migri officer has started reviewing your application. You will be notified here if any action is required.");

        // Navigate to detail — reset chat state before navigating
        _showAccessReasonDialog.Value = false;
        _accessReason.Value           = "";
        _accessReasonError.Value      = "";
        _selectedAppId.Value          = appId;
        _chatMessages.Value           = [];
        _chatStreaming.Value           = false;
        _messageThreadTab.Value       = "ai";
        Navigate("detail");

        // Initialize chat from the click handler (properly awaited in request context)
        // This avoids the fire-and-forget-from-render lost-update bug
        var app = _applications.Value.FirstOrDefault(a => a.ApplicationId == appId);
        if (app != null)
            await InitializeChatAsync(app);
    }

    // ── PDF document loading (returns base64 data URL — no HTTP endpoint needed) ──
    private async Task<string> LoadDocDataUrlAsync(string appId, string filename, PermitReady.ApplicationInput input)
    {
        // 1. Try to load real uploaded bytes from DB
        byte[]? bytes = await PermitReady.PermitReadyDb.GetDocumentBytesAsync(app, appId, filename);

        // 2. Fall back to deterministic dummy PDF generated from application context
        if (bytes == null || bytes.Length == 0)
        {
            var preview = PermitReady.DummyDocumentFactory.GetPreview(filename, input);
            bytes = PermitReady.DummyDocumentFactory.GeneratePdf(preview);
        }

        return $"data:application/pdf;base64,{Convert.ToBase64String(bytes)}";
    }

    // ── Cache uploaded document bytes for DB persistence at commit time ───
    private async Task CacheUploadedDocBytesAsync(string filename, string tempPath)
    {
        try
        {
            var bytes = await System.IO.File.ReadAllBytesAsync(tempPath);
            _uploadedDocBytes.Value[filename] = bytes;
        }
        catch { }
    }

    // ── Seed demo data ────────────────────────────────────────────────────
    private void SeedDemoData()
    {
        if (_applications.Value.Count > 0) return;

        var seeds = new[]
        {
            // ── Work permits ──────────────────────────────────────────────

            // A001: EmployeePermit - complete, green
            MakeApp("A001", "Amir Hassan", "Syrian", "amir.hassan@example.com",
                PermitReady.PermitCategory.EmployeePermit,
                passportExpiry: DateTime.UtcNow.AddDays(520),
                docs: ["employment_contract.pdf", "passport_copy.pdf", "salary_proof.pdf"],
                employer: "Nokia Oyj", jobTitle: "Software Engineer", salary: 3200m, contractRef: "NOK-2025-112"),

            // A002: SpecialistExpert - missing degree
            MakeApp("A002", "Priya Sharma", "Indian", "priya.sharma@example.com",
                PermitReady.PermitCategory.SpecialistExpert,
                passportExpiry: DateTime.UtcNow.AddDays(400),
                docs: ["employment_contract.pdf", "passport_copy.pdf", "salary_proof.pdf"],
                employer: "Wärtsilä Oyj", jobTitle: "Lead Systems Architect", salary: 4500m, contractRef: "WAR-2025-088"),

            // A003: EUBlueCard - complete, green
            MakeApp("A003", "Dmitri Volkov", "Russian", "dmitri.volkov@example.com",
                PermitReady.PermitCategory.EUBlueCard,
                passportExpiry: DateTime.UtcNow.AddDays(600),
                docs: ["employment_contract.pdf", "passport_copy.pdf", "degree_certificate.pdf", "salary_proof.pdf"],
                employer: "KONE Oyj", jobTitle: "Senior Elevator Engineer", salary: 5200m, contractRef: "KONE-2025-044"),

            // A004: Researcher - complete, green
            MakeApp("A004", "Yuki Tanaka", "Japanese", "yuki.tanaka@example.com",
                PermitReady.PermitCategory.Researcher,
                passportExpiry: DateTime.UtcNow.AddDays(700),
                docs: ["hosting_agreement.pdf", "passport_copy.pdf", "research_agreement.pdf", "funding_letter.pdf"],
                employer: "University of Helsinki", jobTitle: "Postdoctoral Researcher", salary: 2500m),

            // A005: SeasonalWorker - near-expiry passport
            MakeApp("A005", "Fatima Al-Rashid", "Moroccan", "fatima.alrashid@example.com",
                PermitReady.PermitCategory.SeasonalWorker,
                passportExpiry: DateTime.UtcNow.AddDays(60),
                docs: ["employment_contract.pdf", "passport_copy.pdf"],
                employer: "FarmWork Oy", jobTitle: "Agricultural Worker", salary: 1300m, contractRef: "FW-2025-201"),

            // A006: SelfEmployed - missing business docs
            MakeApp("A006", "Maria Santos", "Brazilian", "maria.santos@example.com",
                PermitReady.PermitCategory.SelfEmployed,
                passportExpiry: DateTime.UtcNow.AddDays(450),
                docs: ["passport_copy.pdf"],
                employer: "Santos Design Studio", jobTitle: "Freelance Designer", salary: 2200m),

            // A007: EUBlueCard - borderline salary, complete
            MakeApp("A007", "Chen Wei", "Chinese", "chen.wei@example.com",
                PermitReady.PermitCategory.EUBlueCard,
                passportExpiry: DateTime.UtcNow.AddDays(550),
                docs: ["employment_contract.pdf", "passport_copy.pdf", "degree_certificate.pdf", "salary_proof.pdf"],
                employer: "SAP Finland Oy", jobTitle: "Enterprise Consultant", salary: 4867m, contractRef: "SAP-2025-019"),

            // ── Study permits ─────────────────────────────────────────────

            // A008: StudentHigherEd - complete, green
            MakeApp("A008", "Kofi Mensah", "Ghanaian", "kofi.mensah@example.com",
                PermitReady.PermitCategory.StudentHigherEd,
                passportExpiry: DateTime.UtcNow.AddDays(620),
                docs: ["acceptance_letter.pdf", "transcript.pdf", "passport_copy.pdf"],
                uni: "Aalto University", program: "MSc Computer Science", funds: 750m),

            // A009: StudentVocational - funds too low
            MakeApp("A009", "Nadira Okafor", "Nigerian", "nadira.okafor@example.com",
                PermitReady.PermitCategory.StudentVocational,
                passportExpiry: DateTime.UtcNow.AddDays(390),
                docs: ["acceptance_letter.pdf", "passport_copy.pdf"],
                uni: "Omnia Vocational College", program: "Hospitality Management", funds: 400m),

            // A010: ExchangeStudent - complete
            MakeApp("A010", "Laura García", "Spanish", "laura.garcia@example.com",
                PermitReady.PermitCategory.ExchangeStudent,
                passportExpiry: DateTime.UtcNow.AddDays(480),
                docs: ["exchange_agreement.pdf", "acceptance_letter.pdf", "passport_copy.pdf", "home_university_letter.pdf"],
                uni: "University of Turku", program: "Erasmus Exchange — Business", funds: 700m),

            // A011: LanguageCourse - funds ok
            MakeApp("A011", "Ivan Petrov", "Ukrainian", "ivan.petrov@example.com",
                PermitReady.PermitCategory.LanguageCourse,
                passportExpiry: DateTime.UtcNow.AddDays(510),
                docs: ["enrollment_confirmation.pdf", "passport_copy.pdf", "bank_statement.pdf"],
                uni: "Helsinki Language School", funds: 650m),

            // A012: TraineeIntern - missing training agreement
            MakeApp("A012", "Sana Mirza", "Pakistani", "sana.mirza@example.com",
                PermitReady.PermitCategory.TraineeIntern,
                passportExpiry: DateTime.UtcNow.AddDays(420),
                docs: ["passport_copy.pdf", "bank_statement.pdf"],
                employer: "Reaktor Oy", jobTitle: "Software Intern", funds: 600m),

            // ── Family permits ────────────────────────────────────────────

            // A013: SpouseOfFinnish - complete, green
            MakeApp("A013", "Elena Kozlova", "Russian", "elena.kozlova@example.com",
                PermitReady.PermitCategory.SpouseOfFinnish,
                passportExpiry: DateTime.UtcNow.AddDays(580),
                docs: ["marriage_certificate.pdf", "passport_copy.pdf", "sponsor_identity.pdf", "identity_documents.pdf"],
                sponsorName: "Mikko Mäkinen", sponsorPermitNumber: "FIN-CITIZEN-001"),

            // A014: SpouseOfPermitHolder - missing sponsor permit
            MakeApp("A014", "Amara Diallo", "Guinean", "amara.diallo@example.com",
                PermitReady.PermitCategory.SpouseOfPermitHolder,
                passportExpiry: DateTime.UtcNow.AddDays(350),
                docs: ["marriage_certificate.pdf", "passport_copy.pdf"],
                sponsorName: "Ibrahima Diallo"),

            // A015: ChildOfFinnish - complete, green
            MakeApp("A015", "Kai Yamamoto", "Japanese", "kai.yamamoto@example.com",
                PermitReady.PermitCategory.ChildOfFinnish,
                passportExpiry: DateTime.UtcNow.AddDays(670),
                docs: ["birth_certificate.pdf", "passport_copy.pdf", "parent_identity.pdf", "custody_documents.pdf"],
                sponsorName: "Hanna Virtanen"),

            // A016: SpouseOfFinnish - near-expiry passport, missing marriage cert
            MakeApp("A016", "Zara Ahmed", "Pakistani", "zara.ahmed@example.com",
                PermitReady.PermitCategory.SpouseOfFinnish,
                passportExpiry: DateTime.UtcNow.AddDays(55),
                docs: ["passport_copy.pdf"],
                sponsorName: "Jukka Korhonen", sponsorPermitNumber: "FIN-CITIZEN-442"),

            // A017: ParentOfMinorFinnish - complete
            MakeApp("A017", "Sergei Volkov", "Russian", "sergei.volkov@example.com",
                PermitReady.PermitCategory.ParentOfMinorFinnish,
                passportExpiry: DateTime.UtcNow.AddDays(490),
                docs: ["birth_certificate.pdf", "passport_copy.pdf", "custody_documents.pdf", "child_passport.pdf"],
                sponsorName: "Aleksei Volkov"),

            // A018: DependentFamily - missing dependency proof
            MakeApp("A018", "Mei Lin", "Chinese", "mei.lin@example.com",
                PermitReady.PermitCategory.DependentFamily,
                passportExpiry: DateTime.UtcNow.AddDays(430),
                docs: ["passport_copy.pdf"],
                sponsorName: "Wei Zhang", sponsorPermitNumber: "FI-RP-2024-7721"),
        };

        _applications.Value = [.. seeds];
    }

    private PermitReady.StoredApplication MakeApp(
        string id, string name, string nationality, string email,
        PermitReady.PermitCategory category,
        DateTime passportExpiry,
        List<string> docs,
        string? uni = null, string? program = null, decimal? funds = null,
        string? employer = null, string? jobTitle = null, decimal? salary = null, string? contractRef = null,
        string? sponsorName = null, string? sponsorPermitNumber = null)
    {
        var input = new PermitReady.ApplicationInput(
            FullName:              name,
            Nationality:           nationality,
            Email:                 email,
            Category:              category,
            UniversityName:        uni,
            ProgramName:           program,
            FundsAmount:           funds,
            EmployerName:          employer,
            JobTitle:              jobTitle,
            SalaryAmount:          salary,
            EmploymentContractRef: contractRef,
            SponsorName:           sponsorName,
            SponsorPermitNumber:   sponsorPermitNumber,
            UploadedDocuments:     docs,
            PassportExpiry:        passportExpiry
        );
        var result = PermitReady.ScoringService.Score(input);
        return new PermitReady.StoredApplication(id, input, result, DateTime.UtcNow.AddMinutes(-new Random().Next(5, 120)));
    }
}

// Extension helper
internal static class StringExtensions
{
    public static string? NullIfEmpty(this string s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
