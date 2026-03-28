public partial class IkonDemoApp
{
    private void RenderApplicantHome(UIView view)
    {
        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column([Container.Xl2, "py-10 px-4 gap-10 min-h-full"], content: view =>
            {
                // ── Hero ─────────────────────────────────────────────────
                view.Column(["gap-3"], content: view =>
                {
                    view.Text(["text-xs font-semibold uppercase tracking-widest text-muted-foreground"],
                        "Finnish Immigration Service · Migri");
                    view.Text(["text-3xl font-bold tracking-tight font-heading"],
                        "Residence Permit Application Guide");
                    view.Text(["text-muted-foreground max-w-2xl"],
                        "Finland processes thousands of residence permit applications each year. This guide helps you understand what to prepare, what Migri looks for, and how to submit a complete application the first time.");
                });

                // ── How the process works ─────────────────────────────────
                view.Column([Card.Default, "p-6 gap-5"], content: view =>
                {
                    view.Text([Text.H3], "How the Process Works");

                    view.Row(["gap-4 flex-wrap"], content: view =>
                    {
                        ProcessStep(view, "1", "Apply Online",
                            "Submit your application through EnterFinland (enterfinland.fi). Create an account, fill in all sections, and upload supporting documents.",
                            "file-text");

                        ProcessStep(view, "2", "Completeness Check",
                            "PermitReady's AI instantly scores your application for completeness and flags any missing documents or inconsistencies before submission.",
                            "check-circle");

                        ProcessStep(view, "3", "Migri Review",
                            "Your application is routed automatically: complete applications are fast-tracked, others may receive a supplement request or go to specialist review.",
                            "shield");

                        ProcessStep(view, "4", "Decision",
                            "You receive the decision by post and in EnterFinland. Processing typically takes 1–3 months. You can live in Finland while waiting if you applied before your current permit expired.",
                            "mail");
                    });
                });

                // ── Key requirements grid ─────────────────────────────────
                view.Column(["gap-4"], content: view =>
                {
                    view.Text([Text.H3], "Key Requirements by Permit Type");

                    view.Row(["gap-4 flex-wrap"], content: view =>
                    {
                        // Student permit
                        view.Column([Card.Default, "p-5 flex-1 min-w-[280px] gap-3"], content: view =>
                        {
                            view.Row(["items-center gap-2"], content: view =>
                            {
                                view.Box(["w-8 h-8 rounded-md bg-blue-100 flex items-center justify-center shrink-0"], content: view =>
                                    view.Icon(["text-blue-600 w-4 h-4"], name: "graduation-cap"));
                                view.Text(["font-semibold"], "Student Permit");
                            });

                            view.Column(["gap-2"], content: view =>
                            {
                                RequirementItem(view, "Acceptance letter from a Finnish educational institution");
                                RequirementItem(view, "Academic transcripts from previous studies");
                                RequirementItem(view, "Valid passport (must cover the entire study period + 6 months)");
                                RequirementItem(view, $"Proof of funds: minimum €{6720:N0}/year (€{6720/12:N0}/month)");
                                RequirementItem(view, "Health insurance valid in Finland");
                                RequirementItem(view, "Proof of paid tuition fees (if applicable)");
                            });

                            view.Box([Alert.Info, "px-3 py-2 rounded-md border text-xs mt-1"], content: view =>
                                view.Text([], "Processing time: typically 6–8 weeks. Apply at least 3 months before your studies start."));
                        });

                        // Work permit
                        view.Column([Card.Default, "p-5 flex-1 min-w-[280px] gap-3"], content: view =>
                        {
                            view.Row(["items-center gap-2"], content: view =>
                            {
                                view.Box(["w-8 h-8 rounded-md bg-purple-100 flex items-center justify-center shrink-0"], content: view =>
                                    view.Icon(["text-purple-600 w-4 h-4"], name: "briefcase"));
                                view.Text(["font-semibold"], "Work Permit");
                            });

                            view.Column(["gap-2"], content: view =>
                            {
                                RequirementItem(view, "Signed employment contract from a Finnish employer");
                                RequirementItem(view, "Valid passport (must cover the work permit period + 6 months)");
                                RequirementItem(view, $"Salary at or above sector collective agreement (min. €{1500:N0}/month)");
                                RequirementItem(view, "Employer's statement on working conditions (signed)");
                                RequirementItem(view, "Salary slips or payroll confirmation (if already working)");
                                RequirementItem(view, "Proof of professional qualifications (if required for the role)");
                            });

                            view.Box([Alert.Info, "px-3 py-2 rounded-md border text-xs mt-1"], content: view =>
                                view.Text([], "Processing time: typically 2–3 months. Both employer and employee must submit forms."));
                        });
                    });
                });

                // ── Common reasons for rejection ─────────────────────────
                view.Column([Card.Default, "p-6 gap-4"], content: view =>
                {
                    view.Text([Text.H3], "Common Reasons Applications Are Delayed");

                    view.Row(["gap-3 flex-wrap"], content: view =>
                    {
                        RejectionReason(view, "alert-triangle", "Passport expiring soon",
                            "Your passport must be valid for the full permit period plus at least 6 months. Renew it before applying.");

                        RejectionReason(view, "file-x", "Missing documents",
                            "Incomplete applications are the #1 cause of delays. Use PermitReady's checker before submitting to EnterFinland.");

                        RejectionReason(view, "banknote", "Insufficient funds",
                            "Students must show €6,720/year minimum. Work applicants' salary must meet the collective bargaining agreement floor.");

                        RejectionReason(view, "alert-circle", "Inconsistent information",
                            "Details must match exactly across all documents. Discrepancies in names, dates, or amounts trigger manual review.");
                    });
                });

                // ── Important links ───────────────────────────────────────
                view.Column([Card.Default, "p-6 gap-4"], content: view =>
                {
                    view.Text([Text.H3], "Important Resources");

                    view.Row(["gap-3 flex-wrap"], content: view =>
                    {
                        LinkCard(view, "EnterFinland", "The official online portal for submitting permit applications", "enterfinland.fi");
                        LinkCard(view, "Migri.fi", "Finnish Immigration Service — official guidelines and forms", "migri.fi");
                        LinkCard(view, "Migri Helpline", "Phone: +358 295 430 431 · Mon–Fri 8:00–16:00", null);
                        LinkCard(view, "Email Migri", "info@migri.fi for written enquiries", null);
                    });
                });

                // ── Track application ─────────────────────────────────────
                view.Column([Card.Default, "p-6 gap-4"], content: view =>
                {
                    view.Text([Text.H3], "Track Your Application");
                    view.Text(["text-sm text-muted-foreground"],
                        "Enter the reference number shown on your PermitReady confirmation to see your current status.");

                    view.Row(["gap-3 items-end"], content: view =>
                    {
                        view.Column([FormField.Root, "flex-1"], content: view =>
                        {
                            view.Text([FormField.Label], "Application Reference");
                            view.TextField([Input.Default, "font-mono tracking-wider uppercase"],
                                placeholder: "e.g. A001",
                                value: _trackId.Value,
                                onValueChange: async v =>
                                {
                                    _trackId.Value    = v;
                                    _trackError.Value = "";
                                    _trackedApp.Value = null;
                                });
                        });

                        view.Button([Button.PrimaryMd, "shrink-0"], "Track",
                            onClick: async () => TrackApplication());
                    });

                    if (!string.IsNullOrEmpty(_trackError.Value))
                        view.Text([FormField.ErrorText], _trackError.Value);

                    // Tracked result
                    if (_trackedApp.Value != null)
                        RenderTrackedResult(view, _trackedApp.Value);
                });

                // ── CTA ───────────────────────────────────────────────────
                view.Row([Card.Default, "p-6 items-center justify-between gap-4 flex-wrap"], content: view =>
                {
                    view.Column(["gap-1"], content: view =>
                    {
                        view.Text(["font-semibold text-base"], "Ready to check your application?");
                        view.Text(["text-sm text-muted-foreground"],
                            "Fill in your details and instantly see your completeness score and any issues.");
                    });
                    view.Button([Button.PrimaryMd, "shrink-0"], "Start Application",
                        onClick: async () => Navigate("form"));
                });
            });
        });
    }

    // ── Tracking result card ──────────────────────────────────────────────
    private static void RenderTrackedResult(UIView view, PermitReady.StoredApplication app)
    {
        var status = app.Status;

        // ── Application header ────────────────────────────────────────────
        view.Column(["mt-2 gap-4"], content: view =>
        {
            // App identity row
            view.Row(["items-center justify-between gap-2 flex-wrap"], content: view =>
            {
                view.Column(["gap-0.5"], content: view =>
                {
                    view.Row(["items-center gap-2"], content: view =>
                    {
                        view.Text(["font-semibold"], app.Input.FullName);
                        view.Box([Badge.DefaultSm, "text-xs"],
                            content: v => v.Text([], app.Input.Category.ShortName()));
                    });
                    view.Text(["text-xs text-muted-foreground"],
                        $"Ref: {app.ApplicationId} · Submitted {app.SubmittedAt:dd MMM yyyy}");
                });

                // Overall status pill
                var (pillBg, pillText, pillIcon, pillLabel) = status switch
                {
                    PermitReady.ApplicationStatus.PermitIssued       => ("bg-success-primary/15 border-success-primary/30", "text-success-primary", "award",        "Permit Issued"),
                    PermitReady.ApplicationStatus.ReadyForCollection => ("bg-success-primary/15 border-success-primary/30", "text-success-primary", "package",      "Ready to Collect"),
                    PermitReady.ApplicationStatus.PermitInProduction => ("bg-blue-500/10 border-blue-300",                  "text-blue-600",         "printer",      "Permit Printing"),
                    PermitReady.ApplicationStatus.BiometricsComplete => ("bg-blue-500/10 border-blue-300",                  "text-blue-600",         "user-check",   "Biometrics Done"),
                    PermitReady.ApplicationStatus.AwaitingBiometrics => ("bg-primary/10 border-primary/30",                 "text-primary",          "fingerprint",  "Biometrics Needed"),
                    PermitReady.ApplicationStatus.Approved           => ("bg-success-primary/15 border-success-primary/30", "text-success-primary", "check-circle", "Approved"),
                    PermitReady.ApplicationStatus.Rejected           => ("bg-error-primary/15 border-error-primary/30",     "text-error-primary",   "x-circle",     "Not Approved"),
                    PermitReady.ApplicationStatus.SupplementRequested=> ("bg-warning-primary/15 border-warning-primary/30", "text-warning-primary", "alert-circle", "Action Required"),
                    PermitReady.ApplicationStatus.SupplementReceived => ("bg-warning-primary/10 border-warning-primary/20", "text-warning-primary", "clock",        "Response Received"),
                    PermitReady.ApplicationStatus.UnderReview        => ("bg-primary/10 border-primary/30",                 "text-primary",          "search",       "Under Review"),
                    _                                                => ("bg-muted border-border",                           "text-muted-foreground", "clock",        "Screened"),
                };

                view.Row([$"{pillBg} border rounded-full px-3 py-1.5 items-center gap-1.5"], content: view =>
                {
                    view.Icon([$"{pillText} w-3.5 h-3.5"], name: pillIcon);
                    view.Text([$"text-xs font-semibold {pillText}"], pillLabel);
                });
            });

            // ── Rejected state ────────────────────────────────────────────
            if (status == PermitReady.ApplicationStatus.Rejected)
            {
                view.Column([Alert.Danger, "rounded-lg border px-5 py-4 gap-2"], content: view =>
                {
                    view.Row(["items-center gap-2"], content: view =>
                    {
                        view.Icon(["text-error-primary w-4 h-4 shrink-0"], name: "x-circle");
                        view.Text(["font-semibold text-error-primary"], "Application Not Approved");
                    });
                    view.Text(["text-sm text-error-primary/80"],
                        "We're sorry — your application did not meet the requirements at this time.");
                    if (!string.IsNullOrEmpty(app.OfficerNotes))
                        view.Box(["bg-error-primary/10 rounded-md px-3 py-2 mt-1"], content: v =>
                            v.Text(["text-xs italic"], app.OfficerNotes));
                    view.Text(["text-xs text-muted-foreground mt-1"],
                        "You may appeal the decision within 30 days or reapply with corrected documents. Contact Migri for guidance: info@migri.fi");
                });
                return;
            }

            // ── Supplement requested ──────────────────────────────────────
            if (status == PermitReady.ApplicationStatus.SupplementRequested)
            {
                view.Column([Alert.Warning, "rounded-lg border px-5 py-4 gap-2"], content: view =>
                {
                    view.Row(["items-center gap-2"], content: view =>
                    {
                        view.Icon(["text-warning-primary w-4 h-4 shrink-0"], name: "alert-circle");
                        view.Text(["font-semibold text-warning-primary"], "Action Required — Supplement Request");
                    });
                    view.Text(["text-sm text-warning-primary/80"],
                        "Migri has requested additional documents or clarification. Please log in to EnterFinland and upload the requested items.");
                    if (!string.IsNullOrEmpty(app.OfficerNotes))
                        view.Box(["bg-warning-primary/10 rounded-md px-3 py-2 mt-1"], content: v =>
                            v.Text(["text-xs font-medium"], app.OfficerNotes));
                    view.Text(["text-xs text-muted-foreground mt-1"],
                        "⏱ Respond within 30 days to avoid your application being rejected.");
                });
            }

            // ── Progress pipeline ─────────────────────────────────────────
            RenderApplicantPipeline(view, app);

            // ── Current stage guidance ────────────────────────────────────
            RenderStageGuidance(view, app);

            // ── Application details row ───────────────────────────────────
            view.Row(["gap-4 flex-wrap"], content: view =>
            {
                TrackStat(view, "Completeness", $"{app.Result.CompletenessScore}%",
                    app.Result.CompletenessScore >= 90 ? "text-success-primary"
                    : app.Result.CompletenessScore >= 70 ? "text-warning-primary" : "text-error-primary");
                TrackStat(view, "Permit Type", app.Input.Category.ShortName(), "text-foreground");
                TrackStat(view, "Submitted", app.SubmittedAt.ToString("dd MMM yyyy"), "text-foreground");
                if (app.StatusChangedAt.HasValue)
                    TrackStat(view, "Last Update", app.StatusChangedAt.Value.ToString("dd MMM HH:mm"), "text-foreground");
            });
        });
    }

    private static void RenderApplicantPipeline(UIView view, PermitReady.StoredApplication app)
    {
        // Applicant-facing stage definitions
        var stages = new (PermitReady.ApplicationStatus[] ActiveOn, string Icon, string Label, string ETA)[]
        {
            ([PermitReady.ApplicationStatus.Screened],
                "file-check",     "Application Received",        "Immediate"),

            ([PermitReady.ApplicationStatus.Screened, PermitReady.ApplicationStatus.UnderReview,
              PermitReady.ApplicationStatus.SupplementRequested, PermitReady.ApplicationStatus.SupplementReceived],
                "scan",           "Completeness Check",          "Same day"),

            ([PermitReady.ApplicationStatus.UnderReview,
              PermitReady.ApplicationStatus.SupplementRequested, PermitReady.ApplicationStatus.SupplementReceived],
                "user-search",    "Officer Review",              "1–3 months"),

            ([PermitReady.ApplicationStatus.Approved,  PermitReady.ApplicationStatus.AwaitingBiometrics,
              PermitReady.ApplicationStatus.BiometricsComplete, PermitReady.ApplicationStatus.PermitInProduction,
              PermitReady.ApplicationStatus.ReadyForCollection, PermitReady.ApplicationStatus.PermitIssued],
                "check-circle",   "Decision Made",               ""),

            ([PermitReady.ApplicationStatus.AwaitingBiometrics, PermitReady.ApplicationStatus.BiometricsComplete,
              PermitReady.ApplicationStatus.PermitInProduction,  PermitReady.ApplicationStatus.ReadyForCollection,
              PermitReady.ApplicationStatus.PermitIssued],
                "fingerprint",    "Biometrics Appointment",      "Book via Migri"),

            ([PermitReady.ApplicationStatus.PermitInProduction, PermitReady.ApplicationStatus.ReadyForCollection,
              PermitReady.ApplicationStatus.PermitIssued],
                "printer",        "Permit Being Printed",        "2–3 weeks"),

            ([PermitReady.ApplicationStatus.ReadyForCollection, PermitReady.ApplicationStatus.PermitIssued],
                "package",        "Ready for Collection",        "Visit service point"),

            ([PermitReady.ApplicationStatus.PermitIssued],
                "award",          "Permit Issued 🎉",            ""),
        };

        // Determine current stage index for progress bar width
        var status = app.Status;
        int completedCount = 0;
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].ActiveOn.Contains(status) || IsPastStage(status, stages[i].ActiveOn))
                completedCount = i + 1;
        }

        view.Column([Card.Default, "p-5 gap-4"], content: view =>
        {
            view.Row(["items-center gap-2 mb-1"], content: view =>
            {
                view.Icon(["text-primary w-3.5 h-3.5"], name: "route");
                view.Text(["text-xs font-semibold text-primary uppercase tracking-wide"], "Your Journey");
                view.Text(["text-xs text-muted-foreground ml-auto"],
                    $"Step {Math.Min(completedCount, stages.Length)} of {stages.Length}");
            });

            // Progress bar
            view.Box(["w-full h-1.5 bg-muted rounded-full overflow-hidden"], content: view =>
            {
                var pct = (int)((float)completedCount / stages.Length * 100);
                view.Box([$"h-full bg-primary rounded-full transition-all w-[{pct}%]"]);
            });

            // Stage dots row
            view.Row(["gap-0 relative"], content: view =>
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
                    string textColor = isCurrent ? "text-primary font-semibold"
                                     : isDone    ? "text-foreground"
                                     :             "text-muted-foreground";
                    string lineColor = isDone && !isLast ? "bg-primary/30" : "bg-border";

                    view.Column(["flex-1 items-center gap-1.5 relative"], content: view =>
                    {
                        // Connector line before dot (except first)
                        if (i > 0)
                        {
                            view.Box([$"absolute left-0 right-1/2 top-2 h-0.5 {lineColor} -z-0"]);
                        }

                        // Dot
                        view.Box([$"{dotColor} w-4 h-4 rounded-full border-2 z-10 flex items-center justify-center"],
                            content: v =>
                            {
                                if (isDone && !isCurrent)
                                    v.Icon(["w-2 h-2 text-primary"], name: "check");
                            });

                        // Label (only show for current + adjacent)
                        if (isCurrent || i == 0 || isLast)
                        {
                            view.Text([$"text-[9px] text-center leading-tight mt-0.5 {textColor} max-w-[56px]"],
                                label.Replace(" 🎉", ""));
                        }
                    });
                }
            });

            // Current stage detail
            for (int i = 0; i < stages.Length; i++)
            {
                var (activeOn, icon, label, eta) = stages[i];
                if (!IsCurrentApplicantStage(status, activeOn, i, stages)) continue;

                view.Row(["mt-1 bg-primary/5 border border-primary/20 rounded-lg px-4 py-3 items-center gap-3"],
                    content: view =>
                    {
                        view.Box(["w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0"],
                            content: v => v.Icon(["text-primary w-4 h-4"], name: icon));
                        view.Column(["gap-0 flex-1"], content: view =>
                        {
                            view.Text(["text-sm font-semibold text-primary"], label);
                            if (!string.IsNullOrEmpty(eta))
                                view.Text(["text-xs text-muted-foreground"], $"⏱ {eta}");
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
                 "Your application is in the queue. A Migri officer will review it carefully. You don't need to do anything right now — we'll contact you if anything is needed."),

            PermitReady.ApplicationStatus.SupplementReceived =>
                (Alert.Info, "clock",
                 "Response received — thank you",
                 "Your supplementary documents have been received. The officer will continue reviewing your case. No further action needed at this time."),

            PermitReady.ApplicationStatus.Approved =>
                (Alert.Success, "check-circle",
                 "Application approved!",
                 "Congratulations! Your application has been approved. You will soon receive an invitation to book a biometrics appointment at a Migri service point."),

            PermitReady.ApplicationStatus.AwaitingBiometrics =>
                (Alert.Warning, "fingerprint",
                 "Book your biometrics appointment",
                 "You need to visit a Migri service point in person to provide your fingerprints and photo. Log in to EnterFinland to book your appointment slot. Bring your passport and the appointment confirmation."),

            PermitReady.ApplicationStatus.BiometricsComplete =>
                (Alert.Success, "user-check",
                 "Biometrics collected — permit being processed",
                 "Your fingerprints and photo have been recorded. Your physical permit card is now being produced. No further action needed — you'll be notified when it's ready."),

            PermitReady.ApplicationStatus.PermitInProduction =>
                (Alert.Info, "printer",
                 "Your permit card is being printed",
                 "The physical residence permit card is currently being produced. This typically takes 2–3 weeks. You'll receive a notification when it's ready for collection."),

            PermitReady.ApplicationStatus.ReadyForCollection =>
                (Alert.Warning, "package",
                 "Your permit is ready — please collect it",
                 "Your residence permit card is ready at your selected Migri service point. Please visit during opening hours with your passport and appointment confirmation. The card will be returned to Migri if not collected within 60 days."),

            PermitReady.ApplicationStatus.PermitIssued =>
                (Alert.Success, "award",
                 "Welcome to Finland! 🎉",
                 "Your residence permit has been issued. Keep your permit card safe — you'll need it to live and work in Finland. Remember to register your address with the Digital and Population Data Services Agency (DVV) within 7 days of arrival."),

            _ => ("", "", "", ""),
        };

        if (string.IsNullOrEmpty(heading)) return;

        view.Row([$"{alertStyle} rounded-lg border px-4 py-3 items-start gap-3"], content: view =>
        {
            view.Icon(["w-4 h-4 shrink-0 mt-0.5"], name: icon);
            view.Column(["gap-0.5 flex-1"], content: view =>
            {
                view.Text(["text-sm font-semibold"], heading);
                view.Text(["text-sm leading-relaxed opacity-85"], body);
            });
        });
    }

    private static void TrackStat(UIView view, string label, string value, string valueColor)
    {
        view.Column([Card.Default, "px-4 py-3 flex-1 min-w-[100px] gap-0.5"], content: view =>
        {
            view.Text(["text-[10px] text-muted-foreground uppercase tracking-wide"], label);
            view.Text([$"text-sm font-semibold {valueColor}"], value);
        });
    }

    // ── Helpers for applicant pipeline stage logic ────────────────────────

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
        // Supplement states show as "Officer Review" stage (index 2)
        if (stageIndex == 2 && (current == PermitReady.ApplicationStatus.SupplementRequested
                             || current == PermitReady.ApplicationStatus.SupplementReceived))
            return true;
        return false;
    }

    // ── Sub-helpers ───────────────────────────────────────────────────────

    private static void ProcessStep(UIView view, string num, string title, string description, string icon)
    {
        view.Column([Card.Default, "p-4 flex-1 min-w-[200px] gap-3"], content: view =>
        {
            view.Row(["items-center gap-3"], content: view =>
            {
                view.Box(["w-8 h-8 rounded-full bg-primary flex items-center justify-center shrink-0"], content: view =>
                    view.Text(["text-primary-foreground text-xs font-bold"], num));
                view.Icon(["text-muted-foreground w-5 h-5"], name: icon);
            });
            view.Text(["font-semibold text-sm"], title);
            view.Text(["text-xs text-muted-foreground leading-relaxed"], description);
        });
    }

    private static void RequirementItem(UIView view, string text)
    {
        view.Row(["items-start gap-2"], content: view =>
        {
            view.Icon(["text-success-primary w-3.5 h-3.5 mt-0.5 shrink-0"], name: "check");
            view.Text(["text-sm"], text);
        });
    }

    private static void RejectionReason(UIView view, string icon, string title, string description)
    {
        view.Column([Card.Default, "p-4 flex-1 min-w-[200px] gap-2"], content: view =>
        {
            view.Icon(["text-warning-primary w-5 h-5"], name: icon);
            view.Text(["font-semibold text-sm"], title);
            view.Text(["text-xs text-muted-foreground leading-relaxed"], description);
        });
    }

    private static void LinkCard(UIView view, string title, string description, string? url)
    {
        view.Column([Card.Default, "p-4 flex-1 min-w-[160px] gap-1"], content: view =>
        {
            view.Row(["items-center gap-2"], content: view =>
            {
                view.Icon(["text-primary w-4 h-4"], name: "external-link");
                view.Text(["font-semibold text-sm"], title);
            });
            view.Text(["text-xs text-muted-foreground"], description);
            if (url != null)
                view.Text(["text-xs text-primary font-mono"], url);
        });
    }
}
