public partial class IkonDemoApp
{
    // Finnish immigration process stages in order
    private static readonly (PermitReady.ApplicationStatus Status, string Icon, string Label, string Description)[] ProcessStages =
    [
        (PermitReady.ApplicationStatus.Screened,            "scan",           "AI Pre-screening",       "Application assessed by PermitReady"),
        (PermitReady.ApplicationStatus.UnderReview,         "eye",            "Officer Review",          "Case assigned and under review"),
        (PermitReady.ApplicationStatus.Approved,            "check-circle",   "Decision Made",           "Officer recommendation submitted"),
        (PermitReady.ApplicationStatus.AwaitingBiometrics,  "fingerprint",    "Biometrics Appointment",  "Applicant visits Migri service point"),
        (PermitReady.ApplicationStatus.BiometricsComplete,  "user-check",     "Biometrics Complete",     "Fingerprints & photo collected"),
        (PermitReady.ApplicationStatus.PermitInProduction,  "printer",        "Permit Production",       "Physical permit card being printed"),
        (PermitReady.ApplicationStatus.ReadyForCollection,  "package",        "Ready for Collection",    "Permit card ready at service point"),
        (PermitReady.ApplicationStatus.PermitIssued,        "award",          "Permit Issued",           "Residence permit granted"),
    ];

    private void RenderProcessPipeline(UIView view, PermitReady.StoredApplication app)
    {
        var status = app.Status;
        bool isRejected    = status == PermitReady.ApplicationStatus.Rejected;
        bool isSupplement  = status == PermitReady.ApplicationStatus.SupplementRequested
                          || status == PermitReady.ApplicationStatus.SupplementReceived;

        view.Column([Card.Default, "p-4 gap-3"], content: view =>
        {
            // Header
            view.Row(["items-center gap-2 mb-1"], content: view =>
            {
                view.Icon(["text-primary w-3.5 h-3.5"], name: "git-branch");
                view.Text(["text-xs font-semibold text-primary uppercase tracking-wide"],
                    "Immigration Process");
                if (app.StatusChangedAt.HasValue)
                    view.Text(["text-xs text-muted-foreground ml-auto"],
                        $"Updated {app.StatusChangedAt.Value:MMM dd HH:mm}");
            });

            // Rejected state
            if (isRejected)
            {
                view.Row([Alert.Danger, "px-3 py-2.5 rounded-lg border items-center gap-2"], content: view =>
                {
                    view.Icon(["text-error-primary w-4 h-4 shrink-0"], name: "x-circle");
                    view.Column(["gap-0"], content: view =>
                    {
                        view.Text(["text-xs font-semibold text-error-primary"], "Application Rejected");
                        if (!string.IsNullOrEmpty(app.OfficerNotes))
                            view.Text(["text-xs text-error-primary/80 italic mt-0.5"], app.OfficerNotes);
                    });
                });
                return;
            }

            // Supplement state
            if (isSupplement)
            {
                view.Row([Alert.Warning, "px-3 py-2.5 rounded-lg border items-center gap-2"], content: view =>
                {
                    view.Icon(["text-warning-primary w-4 h-4 shrink-0"], name: "alert-circle");
                    view.Column(["gap-0"], content: view =>
                    {
                        view.Text(["text-xs font-semibold text-warning-primary"],
                            status == PermitReady.ApplicationStatus.SupplementRequested
                                ? "Supplement Requested — awaiting applicant response"
                                : "Supplement Received — ready for re-review");
                        if (!string.IsNullOrEmpty(app.OfficerNotes))
                            view.Text(["text-xs text-warning-primary/80 italic mt-0.5"], app.OfficerNotes);
                    });
                });

                // Show advance button if supplement received
                if (status == PermitReady.ApplicationStatus.SupplementReceived)
                {
                    view.Button([Button.OutlineSm, "w-fit gap-1.5"],
                        content: v =>
                        {
                            v.Icon(["w-3 h-3"], name: "refresh-cw");
                            v.Text([], "Mark as Under Review");
                        },
                        onClick: async () => AdvanceApplicationStatus(app.ApplicationId));
                }
                return;
            }

            // Normal pipeline
            view.Column(["gap-0"], content: view =>
            {
                for (int i = 0; i < ProcessStages.Length; i++)
                {
                    var (stageStatus, icon, label, desc) = ProcessStages[i];
                    bool isDone    = IsStatusAtOrPast(status, stageStatus);
                    bool isCurrent = IsCurrentStage(status, stageStatus);
                    bool isLast    = i == ProcessStages.Length - 1;

                    var dotColor = isDone
                        ? (isCurrent ? "bg-primary border-primary" : "bg-primary/40 border-primary/40")
                        : "bg-muted border-border";
                    var lineColor = isDone && !isLast ? "bg-primary/30" : "bg-border";
                    var labelColor = isCurrent ? "text-primary font-semibold"
                                  : isDone    ? "text-foreground"
                                  : "text-muted-foreground";

                    view.Row(["gap-3 items-start"], content: view =>
                    {
                        // Dot + vertical line column
                        view.Column(["items-center w-5 shrink-0"], content: view =>
                        {
                            view.Box([dotColor, "w-4 h-4 rounded-full border-2 flex items-center justify-center shrink-0 mt-0.5"],
                                content: v =>
                                {
                                    if (isDone && !isCurrent)
                                        v.Icon(["w-2 h-2 text-primary"], name: "check");
                                    else if (isCurrent)
                                        v.Box(["w-1.5 h-1.5 rounded-full bg-primary"]);
                                });
                            if (!isLast)
                                view.Box([$"w-0.5 h-5 {lineColor} mx-auto mt-0.5"]);
                        });

                        // Label + desc
                        view.Column(["gap-0 pb-3 flex-1"], content: view =>
                        {
                            view.Row(["items-center gap-1.5"], content: view =>
                            {
                                if (isCurrent)
                                    view.Icon(["text-primary w-3 h-3"], name: icon);
                                view.Text([$"text-xs {labelColor}"], label);

                                // Advance button for actionable stages (demo)
                                if (isCurrent && stageStatus == PermitReady.ApplicationStatus.AwaitingBiometrics)
                                {
                                    view.Button([Button.PrimarySm, "ml-auto text-[10px] h-5 px-2"],
                                        label: "Simulate ✓",
                                        onClick: async () => AdvanceApplicationStatus(app.ApplicationId));
                                }
                                else if (isCurrent && stageStatus == PermitReady.ApplicationStatus.BiometricsComplete)
                                {
                                    view.Button([Button.PrimarySm, "ml-auto text-[10px] h-5 px-2"],
                                        label: "To Production →",
                                        onClick: async () => AdvanceApplicationStatus(app.ApplicationId));
                                }
                                else if (isCurrent && stageStatus == PermitReady.ApplicationStatus.PermitInProduction)
                                {
                                    view.Button([Button.PrimarySm, "ml-auto text-[10px] h-5 px-2"],
                                        label: "Ready →",
                                        onClick: async () => AdvanceApplicationStatus(app.ApplicationId));
                                }
                                else if (isCurrent && stageStatus == PermitReady.ApplicationStatus.ReadyForCollection)
                                {
                                    view.Button([Button.PrimarySm, "ml-auto text-[10px] h-5 px-2"],
                                        label: "Issue Permit ✓",
                                        onClick: async () => AdvanceApplicationStatus(app.ApplicationId));
                                }
                            });

                            if (isCurrent)
                                view.Text(["text-[10px] text-muted-foreground"], desc);
                        });
                    });
                }
            });
        });
    }

    // Maps the current ApplicationStatus to which pipeline stage it's at/past
    private static readonly PermitReady.ApplicationStatus[] StageOrder =
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

    private static bool IsStatusAtOrPast(PermitReady.ApplicationStatus current, PermitReady.ApplicationStatus stage)
    {
        var ci = Array.IndexOf(StageOrder, current);
        var si = Array.IndexOf(StageOrder, stage);
        return ci >= 0 && si >= 0 && ci >= si;
    }

    private static bool IsCurrentStage(PermitReady.ApplicationStatus current, PermitReady.ApplicationStatus stage)
    {
        if (current == stage) return true;
        // UnderReview shows as current for Screened stage too
        if (current == PermitReady.ApplicationStatus.UnderReview && stage == PermitReady.ApplicationStatus.UnderReview)
            return true;
        return false;
    }
}
