public partial class IkonDemoApp
{
    // ── Root: fixed two-panel layout, no page-level scroll ───────────────

    private void RenderApplicantHome(UIView view)
    {
        view.Row(["h-full overflow-hidden"], content: view =>
        {
            // LEFT AREA: campaign info (full) ↔ brand strip + inline form/assessment
            view.Row(["flex-1 min-w-0 h-full overflow-hidden"], content: view =>
            {
                // Campaign/brand column — full width in info mode, narrow strip in assessment/applying
                view.Column([
                    _homePanel.Value == "info" ? "flex-1" : "w-16 shrink-0",
                    "h-full overflow-hidden border-r border-border"
                ], content: view =>
                {
                    if (_homePanel.Value == "info")
                        RenderCampaignPanel(view);
                    else
                        RenderBrandStrip(view);
                });

                // Center content — quiz or form (applying)
                if (_homePanel.Value == "assessment" || _homePanel.Value == "applying")
                    view.Box(["flex-1 min-w-0 h-full overflow-hidden"], content: view =>
                    {
                        if (_homePanel.Value == "assessment")
                            RenderPermitQuizInline(view);
                        else
                            RenderForm(view);
                    });
            });

            // Divider
            view.Box(["w-px bg-border shrink-0"]);

            // RIGHT PANEL: always-visible tracker sidebar
            view.Box(["w-[380px] shrink-0 h-full overflow-hidden"], content: view =>
                RenderTrackerSidebar(view));
        });
    }

    // ── Campaign panel (left 2/3 in info mode) ────────────────────────────

    private void RenderCampaignPanel(UIView view)
    {
        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column(["min-h-full"], content: view =>
            {
                // ── Hero ─────────────────────────────────────────────────
                view.Column(["px-12 py-14 gap-8 bg-[#002F6C]"], content: view =>
                {
                    // Finnish flag + authority label
                    view.Row(["items-center gap-3"], content: view =>
                    {
                        view.Column(["gap-0 w-9 rounded overflow-hidden border border-white/20 shrink-0"], content: view =>
                        {
                            view.Box(["h-3 bg-white"]);
                            view.Box(["h-3.5 bg-[#003F88]"]);
                            view.Box(["h-3 bg-white"]);
                        });
                        view.Column(["gap-0"], content: view =>
                        {
                            view.Text(["text-[10px] text-blue-200 font-bold tracking-[0.2em] uppercase"],
                                "Finnish Immigration Service");
                            view.Text(["text-[10px] text-blue-300/80 tracking-widest"],
                                "Maahanmuuttovirasto · Migri");
                        });
                    });

                    // Campaign tagline
                    view.Column(["gap-3 max-w-2xl"], content: view =>
                    {
                        view.Text(["text-5xl font-bold font-heading leading-[1.05] text-white tracking-tight"],
                            "Smart Immigration.\nFinnish Standards.");
                        view.Text(["text-lg text-blue-200/90 leading-relaxed max-w-xl"],
                            "Finland's immigration system is built on clarity, speed, and fairness. Prepare a complete application the first time — and move forward faster.");
                    });

                    // Stats row
                    view.Row(["gap-6 flex-wrap"], content: view =>
                    {
                        HeroStat(view, "6–8 weeks", "typical decision time");
                        HeroStat(view, "32,000+",   "permits issued annually");
                        HeroStat(view, "100%",       "online applications processed");
                    });

                    // Primary CTA
                    view.Button([
                        "bg-white text-[#002F6C] hover:bg-blue-50 active:scale-[0.98]",
                        "font-bold px-8 py-4 rounded-2xl text-[15px]",
                        "flex flex-row items-center gap-3 w-fit shadow-xl transition-all"
                    ], content: view =>
                    {
                        view.Text([], "Start Your Application");
                        view.Icon(["w-5 h-5"], name: "arrow-right");
                    }, onClick: async () => {
                        _quizPhase.Value = "start";
                        _quizCurrentQuestion.Value = 0;
                        _quizAnswers.Value = [];
                        _homePanel.Value = "assessment";
                    });
                });

                // ── How it works ──────────────────────────────────────────
                view.Column(["px-12 py-10 gap-5 border-b border-border"], content: view =>
                {
                    view.Row(["items-center gap-2.5 mb-1"], content: view =>
                    {
                        view.Box(["w-1 h-5 bg-primary rounded-full shrink-0"]);
                        view.Text(["font-semibold text-base"], "How the process works");
                    });

                    view.Row(["gap-4 flex-wrap"], content: view =>
                    {
                        ProcessCard(view, "1", "apply-online",  "Apply Online",
                            "Fill all sections in EnterFinland, upload supporting documents, and pay the processing fee.", "file-text");
                        ProcessCard(view, "2", "ai-check",      "AI Pre-Check",
                            "PermitReady scores your application for completeness and flags issues before it reaches an officer.", "cpu");
                        ProcessCard(view, "3", "officer-review","Officer Review",
                            "A Migri case officer reviews your file. Complete applications are fast-tracked automatically.", "shield");
                        ProcessCard(view, "4", "decision",      "Decision",
                            "Receive the permit decision in EnterFinland and by post. Typically 6–8 weeks from submission.", "mail");
                    });
                });

                // ── Permit requirements ───────────────────────────────────
                view.Column(["px-12 py-10 gap-5 border-b border-border"], content: view =>
                {
                    view.Row(["items-center gap-2.5 mb-1"], content: view =>
                    {
                        view.Box(["w-1 h-5 bg-primary rounded-full shrink-0"]);
                        view.Text(["font-semibold text-base"], "Key requirements by permit type");
                    });

                    view.Row(["gap-4 flex-wrap"], content: view =>
                    {
                        // Study permit
                        PermitTypeCard(view,
                            icon: "graduation-cap", iconBg: "bg-blue-100", iconColor: "text-blue-600",
                            title: "Study Permit", fee: "€600 online",
                            requirements:
                            [
                                "Acceptance letter from Finnish institution",
                                "Proof of funds: min. €560/month",
                                "Valid passport + academic transcripts",
                                "Health insurance valid in Finland",
                            ],
                            tip: "Apply at least 3 months before studies start");

                        // Work permit
                        PermitTypeCard(view,
                            icon: "briefcase", iconBg: "bg-violet-100", iconColor: "text-violet-600",
                            title: "Work Permit", fee: "€750 online",
                            requirements:
                            [
                                "Signed employment contract (Finnish employer)",
                                "Salary ≥ €1,500/month (sector minimum)",
                                "Valid passport covering permit period + 6 months",
                                "Employer must submit documentation to Migri",
                            ],
                            tip: "Employer must also submit forms to Migri");

                        // Family permit
                        PermitTypeCard(view,
                            icon: "heart", iconBg: "bg-rose-100", iconColor: "text-rose-600",
                            title: "Family Permit", fee: "€750 online",
                            requirements:
                            [
                                "Relationship proof (marriage/birth certificate)",
                                "Sponsor's Finnish permit or citizenship proof",
                                "Valid passport for all family members",
                                "Sponsor demonstrates sufficient income",
                            ],
                            tip: "Sponsor must demonstrate sufficient income");
                    });
                });

                // ── Common pitfalls ───────────────────────────────────────
                view.Column(["px-12 py-10 gap-5"], content: view =>
                {
                    view.Row(["items-center gap-2.5 mb-1"], content: view =>
                    {
                        view.Box(["w-1 h-5 bg-warning-primary rounded-full shrink-0"]);
                        view.Text(["font-semibold text-base"], "Common reasons applications are delayed");
                    });

                    view.Row(["gap-3 flex-wrap"], content: view =>
                    {
                        PitfallCard(view, "alert-triangle", "Expiring passport",
                            "Must be valid for the full permit period plus at least 6 months");
                        PitfallCard(view, "file-x", "Missing documents",
                            "Use PermitReady's completeness checker before submitting");
                        PitfallCard(view, "banknote", "Insufficient funds",
                            "Bank statements must clearly show the required monthly minimums");
                        PitfallCard(view, "alert-circle", "Inconsistent information",
                            "Names, dates, and amounts must match exactly across all documents");
                    });

                    // Bottom CTA
                    view.Row([Card.Default, "px-6 py-5 items-center justify-between gap-4 bg-primary/5 border-primary/20 flex-wrap mt-4"],
                        content: view =>
                    {
                        view.Column(["gap-1"], content: view =>
                        {
                            view.Text(["font-semibold"], "Ready to check your application?");
                            view.Text(["text-sm text-muted-foreground"],
                                "Takes about 10 minutes. Your progress is saved automatically.");
                        });
                        view.Button([Button.PrimaryMd, "shrink-0 gap-2"],
                            content: v =>
                            {
                                v.Text([], "Start Application");
                                v.Icon(["w-4 h-4"], name: "arrow-right");
                            },
                            onClick: async () => {
                                _quizPhase.Value = "start";
                                _quizCurrentQuestion.Value = 0;
                                _quizAnswers.Value = [];
                                _homePanel.Value = "assessment";
                            });
                    });
                });
            });
        });
    }

    // ── Narrow brand strip (applying mode) ────────────────────────────────

    private void RenderBrandStrip(UIView view)
    {
        view.Column(["h-full items-center py-5 gap-4 bg-[#002F6C] overflow-hidden"], content: view =>
        {
            // Close button
            view.Button([
                "w-10 h-10 rounded-xl bg-white/10 hover:bg-white/20 active:bg-white/30",
                "flex items-center justify-center shrink-0 transition-colors"
            ], onClick: async () => { _homePanel.Value = "info"; },
               content: v => v.Icon(["w-4 h-4 text-white"], name: "x"));

            // Migri badge
            view.Box(["w-10 h-10 rounded-xl bg-white flex items-center justify-center shrink-0"],
                content: v => v.Text(["text-[#002F6C] font-black text-sm leading-none"], "Mi"));

            // Spacer + steps progress dots
            view.Column(["flex-1 items-center justify-center gap-3"], content: view =>
            {
                view.Box(["w-2 h-2 rounded-full bg-white shrink-0"]);
                view.Box(["w-px h-5 bg-white/30 shrink-0"]);
                view.Box(["w-2 h-2 rounded-full bg-white/40 shrink-0"]);
                view.Box(["w-px h-5 bg-white/30 shrink-0"]);
                view.Box(["w-2 h-2 rounded-full bg-white/20 shrink-0"]);
                view.Box(["w-px h-5 bg-white/30 shrink-0"]);
                view.Box(["w-2 h-2 rounded-full bg-white/10 shrink-0"]);
            });

            // Mini Finnish flag
            view.Column(["gap-0 w-8 rounded overflow-hidden border border-white/20 shrink-0"], content: view =>
            {
                view.Box(["h-2 bg-white"]);
                view.Box(["h-2.5 bg-[#003F88]"]);
                view.Box(["h-2 bg-white"]);
            });
        });
    }

    // ── Right panel: tracker sidebar ──────────────────────────────────────

    private void RenderTrackerSidebar(UIView view)
    {
        view.Column(["h-full flex flex-col overflow-hidden"], content: view =>
        {
            // Header
            view.Row(["px-5 py-4 border-b border-border items-center gap-3 shrink-0"], content: view =>
            {
                view.Box(["w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"],
                    content: v => v.Icon(["w-4 h-4 text-primary"], name: "search"));
                view.Column(["gap-0 flex-1"], content: view =>
                {
                    view.Text(["font-semibold text-sm"], "Track Your Application");
                    view.Text(["text-xs text-muted-foreground"], "Real-time status from Migri");
                });
                if (_trackedApp.Value != null)
                    view.Button([Button.GhostSm, "text-xs text-muted-foreground shrink-0"], "Clear",
                        onClick: async () =>
                        {
                            _trackedApp.Value = null;
                            _trackId.Value    = "";
                            _trackError.Value = "";
                        });
            });

            // Track input
            view.Column(["px-5 py-4 gap-2 border-b border-border shrink-0"], content: view =>
            {
                view.Row(["gap-2"], content: view =>
                {
                    view.TextField([Input.Default, "font-mono tracking-wider uppercase flex-1"],
                        placeholder: "Reference — e.g. A001",
                        value: _trackId.Value,
                        onValueChange: async v =>
                        {
                            _trackId.Value    = v;
                            _trackError.Value = "";
                            _trackedApp.Value = null;
                        });
                    view.Button([Button.PrimaryMd, "shrink-0"], "Track",
                        onClick: async () => TrackApplication());
                });
                if (!string.IsNullOrEmpty(_trackError.Value))
                    view.Text([FormField.ErrorText, "text-xs"], _trackError.Value);
            });

            // Scrollable result area
            view.ScrollArea(rootStyle: ["flex-1 min-h-0"], content: view =>
            {
                if (_trackedApp.Value != null)
                {
                    var liveApp = _applications.Value
                        .FirstOrDefault(a => a.ApplicationId == _trackedApp.Value.ApplicationId)
                        ?? _trackedApp.Value;

                    view.Column(["px-5 py-4 gap-3"], content: view =>
                        RenderTrackedResult(view, liveApp));
                }
                else
                {
                    // Empty state
                    view.Column(["px-5 py-8 gap-3 items-center text-center"], content: view =>
                    {
                        view.Box(["w-14 h-14 rounded-2xl bg-muted flex items-center justify-center"], content: v =>
                            v.Icon(["w-7 h-7 text-muted-foreground"], name: "file-search"));

                        view.Column(["gap-1"], content: view =>
                        {
                            view.Text(["font-medium text-sm"], "Track your application");
                            view.Text(["text-xs text-muted-foreground max-w-[220px]"],
                                "Enter the reference number shown on your PermitReady confirmation to see your current Migri status.");
                        });
                    });

                    view.Box(["mx-5 h-px bg-border"]);

                    // Resource links
                    view.Column(["px-5 py-4 gap-2"], content: view =>
                    {
                        view.Text(["text-[10px] font-bold text-muted-foreground uppercase tracking-widest mb-1"],
                            "Useful Resources");

                        TrackerResourceItem(view, "EnterFinland",    "enterfinland.fi",         "external-link");
                        TrackerResourceItem(view, "Migri.fi",         "migri.fi",                "external-link");
                        TrackerResourceItem(view, "Migri Helpline",   "+358 295 430 431",        "phone");
                        TrackerResourceItem(view, "Email Migri",      "info@migri.fi",           "mail");
                        TrackerResourceItem(view, "Office hours",     "Mon–Fri  8:00–16:00 EET", "clock");
                    });

                    view.Box(["mx-5 h-px bg-border"]);

                    // Unread messages banner (if applicant has a tracked app with unread msgs)
                    view.Column(["px-5 py-4 gap-2"], content: view =>
                    {
                        view.Text(["text-[10px] font-bold text-muted-foreground uppercase tracking-widest mb-1"],
                            "Processing Times");

                        foreach (var (label, time) in new (string, string)[]
                        {
                            ("Study permit",      "6–8 weeks"),
                            ("Work permit",       "2–3 months"),
                            ("Family permit",     "6–9 months"),
                            ("EU Blue Card",      "2–3 months"),
                            ("Working Holiday",   "2–4 weeks"),
                        })
                        {
                            view.Row(["justify-between items-center py-1.5 border-b border-border/50 last:border-0"], content: view =>
                            {
                                view.Text(["text-xs"], label);
                                view.Text(["text-xs font-medium text-primary"], time);
                            });
                        }
                    });
                }
            });
        });
    }

    // ── Tracking result card ──────────────────────────────────────────────

    private void RenderTrackedResult(UIView view, PermitReady.StoredApplication app)
    {
        var status = app.Status;

        // App identity
        view.Row(["items-start justify-between gap-2 flex-wrap"], content: view =>
        {
            view.Column(["gap-0.5 flex-1"], content: view =>
            {
                view.Text(["font-semibold text-sm"], app.Input.FullName);
                view.Text(["text-xs text-muted-foreground"],
                    $"{app.ApplicationId} · {app.SubmittedAt:dd MMM yyyy}");
            });
            view.Box([Badge.DefaultSm, "text-[10px] shrink-0"],
                content: v => v.Text([], app.Input.Category.ShortName()));
        });

        // Payment status
        if (app.FeeAmount > 0)
        {
            view.Row(["bg-success-primary/10 border border-success rounded-lg px-3 py-2 items-center gap-2"], content: view =>
            {
                view.Icon(["w-3.5 h-3.5 text-success-primary shrink-0"], name: "check-circle");
                view.Text(["text-xs font-medium text-success-primary"], $"€{app.FeeAmount:N0} paid");
                if (!string.IsNullOrEmpty(app.PaymentReference))
                    view.Text(["text-[10px] text-muted-foreground ml-auto font-mono"], app.PaymentReference);
            });
        }

        // Status pill
        var (pillBg, pillText, pillIcon, pillLabel) = status switch
        {
            PermitReady.ApplicationStatus.PermitIssued        => ("bg-success-primary/15 border-success-primary/30", "text-success-primary", "award",         "Permit Issued"),
            PermitReady.ApplicationStatus.ReadyForCollection  => ("bg-success-primary/15 border-success-primary/30", "text-success-primary", "package",       "Ready to Collect"),
            PermitReady.ApplicationStatus.PermitInProduction  => ("bg-blue-500/10 border-blue-300",                  "text-blue-600",        "printer",       "Permit Printing"),
            PermitReady.ApplicationStatus.BiometricsComplete  => ("bg-blue-500/10 border-blue-300",                  "text-blue-600",        "user-check",    "Biometrics Done"),
            PermitReady.ApplicationStatus.AwaitingBiometrics  => ("bg-primary/10 border-primary/30",                 "text-primary",         "fingerprint",   "Book Biometrics"),
            PermitReady.ApplicationStatus.Approved            => ("bg-success-primary/15 border-success-primary/30", "text-success-primary", "check-circle",  "Approved"),
            PermitReady.ApplicationStatus.Rejected            => ("bg-error-primary/15 border-error-primary/30",     "text-error-primary",   "x-circle",      "Not Approved"),
            PermitReady.ApplicationStatus.SupplementRequested => ("bg-warning-primary/15 border-warning-primary/30", "text-warning-primary", "alert-circle",  "Action Required"),
            PermitReady.ApplicationStatus.SupplementReceived  => ("bg-warning-primary/10 border-warning-primary/20", "text-warning-primary", "clock",         "Response Received"),
            PermitReady.ApplicationStatus.UnderReview         => ("bg-primary/10 border-primary/30",                 "text-primary",         "search",        "Under Review"),
            _                                                 => ("bg-muted border-border",                          "text-muted-foreground","clock",         "Received"),
        };

        view.Row([$"{pillBg} border rounded-full px-3 py-1.5 items-center gap-1.5 w-fit"], content: view =>
        {
            view.Icon([$"{pillText} w-3 h-3"], name: pillIcon);
            view.Text([$"text-xs font-semibold {pillText}"], pillLabel);
        });

        // Alert states
        if (status == PermitReady.ApplicationStatus.Rejected)
        {
            view.Column([Alert.Danger, "rounded-lg border px-4 py-3 gap-1.5"], content: view =>
            {
                view.Text(["font-semibold text-sm text-error-primary"], "Application Not Approved");
                view.Text(["text-xs text-error-primary/80"],
                    "You may appeal within 30 days or reapply with corrected documents.");
                if (!string.IsNullOrEmpty(app.OfficerNotes))
                    view.Box(["bg-error-primary/10 rounded px-2.5 py-1.5 mt-1"],
                        content: v => v.Text(["text-xs italic"], app.OfficerNotes));
            });
            goto afterAlerts;
        }

        if (status == PermitReady.ApplicationStatus.SupplementRequested)
        {
            view.Column([Alert.Warning, "rounded-lg border px-4 py-3 gap-1.5"], content: view =>
            {
                view.Text(["font-semibold text-sm text-warning-primary"], "Action Required — Supplement Request");
                view.Text(["text-xs text-warning-primary/80"],
                    "Log in to EnterFinland and upload the requested documents within 30 days.");
                if (!string.IsNullOrEmpty(app.OfficerNotes))
                    view.Box(["bg-warning-primary/10 rounded px-2.5 py-1.5 mt-1"],
                        content: v => v.Text(["text-xs font-medium"], app.OfficerNotes));
            });
        }

        afterAlerts:

        // Progress pipeline
        RenderApplicantPipeline(view, app);

        // Stage guidance
        RenderStageGuidance(view, app);

        // Messages
        RenderApplicantMessageThread(view, app);

        // Stats
        view.Row(["gap-2 flex-wrap"], content: view =>
        {
            TrackStat(view, "Completeness", $"{app.Result.CompletenessScore}%",
                app.Result.CompletenessScore >= 90 ? "text-success-primary"
                : app.Result.CompletenessScore >= 70 ? "text-warning-primary" : "text-error-primary");
            TrackStat(view, "Permit", app.Input.Category.ShortName(), "text-foreground");
            TrackStat(view, "Submitted", app.SubmittedAt.ToString("dd MMM"), "text-foreground");
        });
    }

    private static void RenderApplicantPipeline(UIView view, PermitReady.StoredApplication app)
    {
        var stages = new (PermitReady.ApplicationStatus[] ActiveOn, string Icon, string Label, string ETA)[]
        {
            ([PermitReady.ApplicationStatus.Screened],
                "file-check",  "Received",        "Immediate"),
            ([PermitReady.ApplicationStatus.Screened, PermitReady.ApplicationStatus.UnderReview,
              PermitReady.ApplicationStatus.SupplementRequested, PermitReady.ApplicationStatus.SupplementReceived],
                "scan",        "Checked",         "Same day"),
            ([PermitReady.ApplicationStatus.UnderReview,
              PermitReady.ApplicationStatus.SupplementRequested, PermitReady.ApplicationStatus.SupplementReceived],
                "user-search", "Officer Review",  "1–3 months"),
            ([PermitReady.ApplicationStatus.Approved, PermitReady.ApplicationStatus.AwaitingBiometrics,
              PermitReady.ApplicationStatus.BiometricsComplete, PermitReady.ApplicationStatus.PermitInProduction,
              PermitReady.ApplicationStatus.ReadyForCollection, PermitReady.ApplicationStatus.PermitIssued],
                "check-circle","Decision",        ""),
            ([PermitReady.ApplicationStatus.AwaitingBiometrics, PermitReady.ApplicationStatus.BiometricsComplete,
              PermitReady.ApplicationStatus.PermitInProduction, PermitReady.ApplicationStatus.ReadyForCollection,
              PermitReady.ApplicationStatus.PermitIssued],
                "fingerprint", "Biometrics",      "Book via Migri"),
            ([PermitReady.ApplicationStatus.PermitInProduction, PermitReady.ApplicationStatus.ReadyForCollection,
              PermitReady.ApplicationStatus.PermitIssued],
                "printer",     "Printing",        "2–3 weeks"),
            ([PermitReady.ApplicationStatus.ReadyForCollection, PermitReady.ApplicationStatus.PermitIssued],
                "package",     "Collection",      "Service point"),
            ([PermitReady.ApplicationStatus.PermitIssued],
                "award",       "Issued",          ""),
        };

        var status = app.Status;
        int completedCount = 0;
        for (int i = 0; i < stages.Length; i++)
            if (stages[i].ActiveOn.Contains(status) || IsPastStage(status, stages[i].ActiveOn))
                completedCount = i + 1;

        view.Column([Card.Default, "p-4 gap-3"], content: view =>
        {
            view.Row(["items-center gap-2"], content: view =>
            {
                view.Icon(["text-primary w-3.5 h-3.5"], name: "route");
                view.Text(["text-xs font-semibold text-primary uppercase tracking-wide flex-1"], "Your Journey");
                view.Text(["text-[10px] text-muted-foreground"],
                    $"{Math.Min(completedCount, stages.Length)}/{stages.Length}");
            });

            view.Box(["w-full h-1 bg-muted rounded-full overflow-hidden"], content: view =>
            {
                var pct = (int)((float)completedCount / stages.Length * 100);
                view.Box([$"h-full bg-primary rounded-full w-[{pct}%]"]);
            });

            view.Row(["gap-0"], content: view =>
            {
                for (int i = 0; i < stages.Length; i++)
                {
                    var (activeOn, icon, label, eta) = stages[i];
                    bool isDone    = IsPastStage(status, activeOn) || activeOn.Contains(status);
                    bool isCurrent = IsCurrentApplicantStage(status, activeOn, i, stages);
                    bool isLast    = i == stages.Length - 1;

                    string dotColor = isCurrent ? "bg-primary border-primary shadow-md shadow-primary/30"
                                    : isDone    ? "bg-primary/40 border-primary/40"
                                    :             "bg-muted border-border";
                    string lineColor = isDone && !isLast ? "bg-primary/30" : "bg-border";

                    view.Column(["flex-1 items-center gap-1 relative"], content: view =>
                    {
                        if (i > 0)
                            view.Box([$"absolute left-0 right-1/2 top-1.5 h-0.5 {lineColor} -z-0"]);

                        view.Box([$"{dotColor} w-3 h-3 rounded-full border-2 z-10 flex items-center justify-center"],
                            content: v =>
                            {
                                if (isDone && !isCurrent)
                                    v.Icon(["w-1.5 h-1.5 text-primary"], name: "check");
                            });

                        if (isCurrent || i == 0 || isLast)
                            view.Text(["text-[8px] text-center leading-tight text-muted-foreground max-w-[40px]"], label);
                    });
                }
            });

            // Current stage highlight
            for (int i = 0; i < stages.Length; i++)
            {
                var (activeOn, icon, label, eta) = stages[i];
                if (!IsCurrentApplicantStage(status, activeOn, i, stages)) continue;
                view.Row(["bg-primary/5 border border-primary/20 rounded-lg px-3 py-2 items-center gap-2.5 mt-1"],
                    content: view =>
                    {
                        view.Box(["w-7 h-7 rounded-full bg-primary/10 flex items-center justify-center shrink-0"],
                            content: v => v.Icon(["text-primary w-3.5 h-3.5"], name: icon));
                        view.Column(["gap-0"], content: view =>
                        {
                            view.Text(["text-xs font-semibold text-primary"], label);
                            if (!string.IsNullOrEmpty(eta))
                                view.Text(["text-[10px] text-muted-foreground"], $"⏱ {eta}");
                        });
                    });
                break;
            }
        });
    }

    private static void RenderStageGuidance(UIView view, PermitReady.StoredApplication app)
    {
        var (alertStyle, icon, heading, body) = app.Status switch
        {
            PermitReady.ApplicationStatus.Screened or
            PermitReady.ApplicationStatus.UnderReview =>
                (Alert.Info, "info",
                 "What happens next?",
                 "Your application is in the queue. You'll be notified here if anything is needed."),

            PermitReady.ApplicationStatus.SupplementReceived =>
                (Alert.Info, "clock",
                 "Response received",
                 "Your documents were received. The officer will continue reviewing. No further action needed."),

            PermitReady.ApplicationStatus.Approved =>
                (Alert.Success, "check-circle",
                 "Application approved!",
                 "You'll receive an invitation to book a biometrics appointment soon."),

            PermitReady.ApplicationStatus.AwaitingBiometrics =>
                (Alert.Warning, "fingerprint",
                 "Book your biometrics appointment",
                 "Visit a Migri service point with your passport. Log in to EnterFinland to book."),

            PermitReady.ApplicationStatus.BiometricsComplete =>
                (Alert.Success, "user-check",
                 "Biometrics collected",
                 "Your permit card is being produced. You'll be notified when it's ready."),

            PermitReady.ApplicationStatus.PermitInProduction =>
                (Alert.Info, "printer",
                 "Permit being printed",
                 "Your permit card is being produced — typically 2–3 weeks."),

            PermitReady.ApplicationStatus.ReadyForCollection =>
                (Alert.Warning, "package",
                 "Permit ready — collect it now",
                 "Visit your selected Migri service point with your passport. Cards are returned after 60 days."),

            PermitReady.ApplicationStatus.PermitIssued =>
                (Alert.Success, "award",
                 "Welcome to Finland! 🎉",
                 "Your permit is issued. Register your address with DVV within 7 days of arrival."),

            _ => ("", "", "", ""),
        };

        if (string.IsNullOrEmpty(heading)) return;

        view.Row([$"{alertStyle} rounded-lg border px-3 py-2.5 items-start gap-2.5"], content: view =>
        {
            view.Icon(["w-3.5 h-3.5 shrink-0 mt-0.5"], name: icon);
            view.Column(["gap-0.5"], content: view =>
            {
                view.Text(["text-xs font-semibold"], heading);
                view.Text(["text-xs leading-relaxed opacity-85"], body);
            });
        });
    }

    private static void TrackStat(UIView view, string label, string value, string valueColor)
    {
        view.Column([Card.Default, "px-3 py-2 flex-1 min-w-[80px] gap-0"], content: view =>
        {
            view.Text(["text-[9px] text-muted-foreground uppercase tracking-wide"], label);
            view.Text([$"text-xs font-semibold {valueColor}"], value);
        });
    }

    // ── Pipeline stage logic ──────────────────────────────────────────────

    private static readonly PermitReady.ApplicationStatus[] ApplicantStageOrder =
    [
        PermitReady.ApplicationStatus.Screened,
        PermitReady.ApplicationStatus.UnderReview,
        PermitReady.ApplicationStatus.Approved,
        PermitReady.ApplicationStatus.AwaitingBiometrics,
        PermitReady.ApplicationStatus.BiometricsComplete,
        PermitReady.ApplicationStatus.PermitInProduction,
        PermitReady.ApplicationStatus.ReadyForCollection,
        PermitReady.ApplicationStatus.PermitIssued,
    ];

    private static bool IsPastStage(
        PermitReady.ApplicationStatus current,
        PermitReady.ApplicationStatus[] stageStatuses)
    {
        var ci = Array.IndexOf(ApplicantStageOrder, current);
        foreach (var s in stageStatuses)
        {
            var si = Array.IndexOf(ApplicantStageOrder, s);
            if (ci > si && si >= 0) return true;
        }
        return false;
    }

    private static bool IsCurrentApplicantStage(
        PermitReady.ApplicationStatus current,
        PermitReady.ApplicationStatus[] stageStatuses,
        int stageIndex,
        (PermitReady.ApplicationStatus[] ActiveOn, string Icon, string Label, string ETA)[] allStages)
    {
        if (stageStatuses.Contains(current)) return true;
        if (stageIndex == 2 && (current == PermitReady.ApplicationStatus.SupplementRequested
                             || current == PermitReady.ApplicationStatus.SupplementReceived))
            return true;
        return false;
    }

    // ── Campaign panel helpers ────────────────────────────────────────────

    private static void HeroStat(UIView view, string value, string label)
    {
        view.Column(["gap-0.5"], content: view =>
        {
            view.Text(["text-xl font-bold text-white font-heading"], value);
            view.Text(["text-xs text-blue-200/80"], label);
        });
    }

    private static void ProcessCard(UIView view, string num, string key, string title, string desc, string icon)
    {
        view.Column([Card.Default, "p-4 flex-1 min-w-[200px] gap-2.5"], content: view =>
        {
            view.Row(["items-center gap-2.5"], content: view =>
            {
                view.Box(["w-7 h-7 rounded-full bg-primary flex items-center justify-center shrink-0"],
                    content: v => v.Text(["text-primary-foreground text-[11px] font-bold"], num));
                view.Icon(["text-muted-foreground w-4 h-4"], name: icon);
            });
            view.Text(["font-semibold text-sm"], title);
            view.Text(["text-xs text-muted-foreground leading-relaxed"], desc);
        });
    }

    private static void PermitTypeCard(UIView view,
        string icon, string iconBg, string iconColor,
        string title, string fee, string[] requirements, string tip)
    {
        view.Column([Card.Default, "p-5 flex-1 min-w-[240px] gap-3"], content: view =>
        {
            view.Row(["items-center gap-2.5 justify-between"], content: view =>
            {
                view.Row(["items-center gap-2"], content: view =>
                {
                    view.Box([$"w-8 h-8 {iconBg} rounded-lg flex items-center justify-center shrink-0"],
                        content: v => v.Icon([$"{iconColor} w-4 h-4"], name: icon));
                    view.Text(["font-semibold text-sm"], title);
                });
                view.Box([Badge.DefaultSm, "text-[10px] shrink-0"],
                    content: v => v.Text([], fee));
            });
            view.Column(["gap-1.5"], content: view =>
            {
                foreach (var req in requirements)
                    RequirementItem(view, req);
            });
            view.Box([Alert.Info, "px-3 py-2 rounded-lg border text-xs mt-1"],
                content: v => v.Text([], tip));
        });
    }

    private static void PitfallCard(UIView view, string icon, string title, string desc)
    {
        view.Column([Card.Default, "p-4 flex-1 min-w-[190px] gap-1.5"], content: view =>
        {
            view.Row(["items-center gap-2"], content: view =>
            {
                view.Icon(["text-warning-primary w-4 h-4 shrink-0"], name: icon);
                view.Text(["font-semibold text-sm"], title);
            });
            view.Text(["text-xs text-muted-foreground leading-relaxed"], desc);
        });
    }

    private static void RequirementItem(UIView view, string text)
    {
        view.Row(["items-start gap-2"], content: view =>
        {
            view.Icon(["text-success-primary w-3.5 h-3.5 mt-0.5 shrink-0"], name: "check");
            view.Text(["text-xs leading-relaxed"], text);
        });
    }

    private static void TrackerResourceItem(UIView view, string label, string value, string icon)
    {
        view.Row([Card.Default, "px-3 py-2.5 items-center gap-2.5"], content: view =>
        {
            view.Icon(["w-3.5 h-3.5 text-primary shrink-0"], name: icon);
            view.Column(["gap-0 flex-1"], content: view =>
            {
                view.Text(["text-xs font-medium"], label);
                view.Text(["text-[10px] text-muted-foreground"], value);
            });
        });
    }
}
