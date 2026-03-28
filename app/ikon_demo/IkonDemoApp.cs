return await App.Run(args);

public record SessionIdentity(string UserId);
public record ClientParameters(string Name = "PermitReady");

[App]
public partial class IkonDemoApp(IApp<SessionIdentity, ClientParameters> app)
{
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
    // Upload slots — one per required document
    private readonly ClientReactive<PermitReady.UploadedDoc?> _passportDoc      = new(null);
    private readonly ClientReactive<PermitReady.UploadedDoc?> _acceptanceDoc    = new(null);
    private readonly ClientReactive<PermitReady.UploadedDoc?> _transcriptDoc    = new(null);
    private readonly ClientReactive<PermitReady.UploadedDoc?> _bankStatementDoc = new(null);
    private readonly ClientReactive<PermitReady.UploadedDoc?> _contractDoc      = new(null);
    private readonly ClientReactive<PermitReady.UploadedDoc?> _salaryProofDoc   = new(null);
    // Drag-active state per zone
    private readonly ClientReactive<bool>    _dragPassport      = new(false);
    private readonly ClientReactive<bool>    _dragAcceptance    = new(false);
    private readonly ClientReactive<bool>    _dragTranscript    = new(false);
    private readonly ClientReactive<bool>    _dragBankStatement = new(false);
    private readonly ClientReactive<bool>    _dragContract      = new(false);
    private readonly ClientReactive<bool>    _dragSalaryProof   = new(false);
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

    // Dashboard tab
    private readonly ClientReactive<string>  _dashboardTab   = new("all");

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

    // ── Officer decision state (per-client) ───────────────────────────────
    private readonly ClientReactive<bool>   _showDecisionPanel  = new(false);
    private readonly ClientReactive<string> _pendingDecision    = new("");   // "approve"|"supplement"|"reject"
    private readonly ClientReactive<string> _decisionNote       = new("");
    private readonly ClientReactive<bool>   _decisionSubmitting = new(false);

    // ── Audit log panel (per-client) ──────────────────────────────────────
    private readonly ClientReactive<bool>   _showAuditLog       = new(false);

    // ── Document viewer state (per-client) ────────────────────────────────
    private string _docsEndpointUrl = "";
    private readonly ClientReactive<bool>   _docViewerOpen   = new(false);
    private readonly ClientReactive<string> _viewingDocAppId = new("");
    private readonly ClientReactive<string> _viewingDocFile  = new("");

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

        // PDF document endpoint — serves generated PDFs at /pdf?appId=...&file=...
        var docsEndpoint = new AppEndpointHost(app, "docs");
        docsEndpoint.MapGet("/pdf", async ctx =>
        {
            var appId    = ctx.Request.Query["appId"].ToString();
            var filename = ctx.Request.Query["file"].ToString();
            var application = _applications.Value.FirstOrDefault(a => a.ApplicationId == appId);
            if (application == null || string.IsNullOrEmpty(filename))
            {
                ctx.Response.StatusCode = 404;
                return;
            }
            var preview  = PermitReady.DummyDocumentFactory.GetPreview(filename, application.Input);
            var pdfBytes = PermitReady.DummyDocumentFactory.GeneratePdf(preview);
            ctx.Response.ContentType = "application/pdf";
            ctx.Response.Headers["Content-Disposition"] = $"inline; filename=\"{filename}\"";
            await ctx.Response.Body.WriteAsync(pdfBytes);
        });
        await docsEndpoint.StartAsync();
        _docsEndpointUrl = docsEndpoint.PublicUrl;

        SeedDemoData();

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
                        case "form":           RenderForm(view);            break;
                        case "review":         RenderReview(view);          break;
                        case "results":        RenderResults(view);         break;
                        case "dashboard":      RenderDashboard(view);       break;
                        case "detail":         RenderDetail(view);          break;
                    }
                });

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
        _page.Value = page;
        _ = ClientFunctions.SetUrlAsync(PageToPath(page)); // fire-and-forget; just updates address bar
    }

    // ── URL ↔ page mapping ────────────────────────────────────────────────
    private static string PageToPath(string page) => page switch
    {
        "landing"        => "/",
        "applicant_home" => "/home",
        "form"           => "/apply",
        "review"         => "/review",
        "results"        => "/results",
        "dashboard"      => "/dashboard",
        "detail"         => "/detail",
        _                => "/"
    };

    private static string? PathToPage(string path) => path.TrimStart('/') switch
    {
        "" or "landing"  => "landing",
        "home"           => "applicant_home",
        "apply"          => "form",
        "review"         => "review",
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
            void AddDoc(PermitReady.UploadedDoc? d) { if (d != null && d.Status != PermitReady.DocStatus.Failed) docs.Add(d.FileName); }
            AddDoc(_passportDoc.Value);
            AddDoc(_acceptanceDoc.Value);
            AddDoc(_transcriptDoc.Value);
            AddDoc(_bankStatementDoc.Value);
            AddDoc(_contractDoc.Value);
            AddDoc(_salaryProofDoc.Value);

            // Use AI-extracted expiry date as fallback if user didn't type one
            if (passportExpiry == null && _passportDoc.Value?.Extracted.TryGetValue("ExpiryDate", out var aiExpiry) == true)
                passportExpiry = DateTime.TryParse(aiExpiry, out var dt2) ? dt2 : null;

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
                PassportExpiry:        passportExpiry
            );

            var result = PermitReady.ScoringService.Score(input);

            // Store scored input for review/results pages
            _lastInput.Value  = input;
            _lastResult.Value = result;

            // If any issues exist → go to review page so applicant can fix before sending
            bool hasIssues = result.MissingItems.Count > 0 || result.RiskFlags.Count > 0;
            if (hasIssues)
            {
                Navigate("review");
            }
            else
            {
                // Perfect application — commit immediately and show results
                CommitApplication(input, result);
                Navigate("results");
            }
        }
        finally
        {
            _isSubmitting.Value = false;
        }
    }

    // ── Phase 2: Applicant confirmed — commit to officer queue ────────────
    private void ConfirmSubmissionAsync()
    {
        var input  = _lastInput.Value;
        var result = _lastResult.Value;
        if (input == null || result == null) return;

        CommitApplication(input, result);
        Navigate("results");
    }

    // ── Internal: write to shared applications list ───────────────────────
    private void CommitApplication(PermitReady.ApplicationInput input, PermitReady.ScreeningResult result)
    {
        var appId  = $"A{(_applications.Value.Count + 1):D3}";
        var stored = new PermitReady.StoredApplication(appId, input, result, DateTime.UtcNow);
        _applications.Value = [.. _applications.Value, stored];
        _lastAppId.Value    = appId;
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
        apps[idx] = apps[idx] with
        {
            Status          = newStatus,
            StatusChangedAt = DateTime.UtcNow,
            OfficerNotes    = officerNotes ?? apps[idx].OfficerNotes,
        };
        _applications.Value = apps;
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
file static class StringExtensions
{
    public static string? NullIfEmpty(this string s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
