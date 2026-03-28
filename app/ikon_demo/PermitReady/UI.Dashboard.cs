public partial class IkonDemoApp
{
    private void RenderDashboard(UIView view)
    {
        var apps = _applications.Value;

        var fastTrack  = apps.Where(a => a.Result.Routing == PermitReady.RoutingType.FastTrack).ToList();
        var supplement = apps.Where(a => a.Result.Routing == PermitReady.RoutingType.SupplementRequested).ToList();
        var specialist = apps.Where(a => a.Result.Routing == PermitReady.RoutingType.SpecialistReview).ToList();

        int avgRisk = apps.Count > 0 ? (int)apps.Average(a => a.Result.RiskScore) : 0;
        int avgComp = apps.Count > 0 ? (int)apps.Average(a => a.Result.CompletenessScore) : 0;

        view.Column(["h-full flex flex-col"], content: view =>
        {
            // Dashboard header
            view.Row(["px-8 py-5 border-b border-border justify-between items-center"], content: view =>
            {
                view.Column(["gap-0.5"], content: view =>
                {
                    view.Text([Text.H2], "Applications Dashboard");
                    view.Text(["text-sm text-muted-foreground"], $"{apps.Count} total applications · Avg risk {avgRisk}/100 · Avg completeness {avgComp}/100");
                });
            });

            // KPI cards
            view.Row(["px-8 py-4 gap-4 border-b border-border flex-wrap"], content: view =>
            {
                KpiCard(view, apps.Count.ToString(),     "Total",         "layers",       "text-foreground");
                KpiCard(view, fastTrack.Count.ToString(), "Fast Track",   "zap",          "text-success-primary");
                KpiCard(view, supplement.Count.ToString(), "Supplement",  "alert-circle", "text-warning-primary");
                KpiCard(view, specialist.Count.ToString(), "Specialist",  "user-check",   "text-error-primary");
            });

            // Tabs with application lists
            view.Box(["flex-1 overflow-hidden px-8 py-4"], content: view =>
            {
                view.Tabs(
                    value: _dashboardTab.Value,
                    onValueChange: async v => { _dashboardTab.Value = v ?? "all"; },
                    listStyle: [Tabs.List],
                    triggerStyle: [Tabs.Trigger],
                    contentStyle: [Tabs.Content, "h-[calc(100vh-280px)]"],
                    tabs:
                    [
                        new TabItem("all",        "All",          v => RenderAppTable(v, apps)),
                        new TabItem("fasttrack",  "Fast Track ✓", v => RenderAppTable(v, fastTrack)),
                        new TabItem("supplement", "Supplement ⚠", v => RenderAppTable(v, supplement)),
                        new TabItem("specialist", "Specialist ✗", v => RenderAppTable(v, specialist)),
                    ]);
            });
        });
    }

    private void RenderAppTable(UIView view, List<PermitReady.StoredApplication> apps)
    {
        if (apps.Count == 0)
        {
            view.Column(["items-center py-16 gap-2"], content: view =>
            {
                view.Icon(["w-10 h-10 text-muted-foreground"], name: "inbox");
                view.Text(["text-muted-foreground"], "No applications in this category.");
            });
            return;
        }

        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column(["gap-2 py-2"], content: view =>
            {
                foreach (var app in apps.OrderByDescending(a => a.SubmittedAt))
                    RenderAppRow(view, app);
            });
        });
    }

    private void RenderAppRow(UIView view, PermitReady.StoredApplication app)
    {
        var (routingColor, routingLabel) = app.Result.Routing switch
        {
            PermitReady.RoutingType.FastTrack           => ("bg-success-primary/20 text-success-primary border-success", "Fast Track"),
            PermitReady.RoutingType.SupplementRequested => ("bg-warning-primary/20 text-warning-primary border-warning", "Supplement"),
            _                                           => ("bg-error-primary/20 text-error-primary border-error",    "Specialist"),
        };

        view.Row([Card.Default, "px-4 py-3 items-center gap-4 cursor-pointer hover:bg-muted/50 transition-colors"], content: view =>
        {
            // ID
            view.Text(["text-xs font-mono text-muted-foreground w-12 shrink-0"], app.ApplicationId);

            // Name + permit type
            view.Column(["flex-1 gap-0"], content: view =>
            {
                view.Text(["font-medium text-sm"], app.Input.FullName);
                view.Text(["text-xs text-muted-foreground"],
                    $"{app.Input.Nationality} · {app.Input.Category.ShortName()}");
            });

            // Scores
            view.Row(["gap-3 items-center shrink-0"], content: view =>
            {
                view.Column(["items-center gap-0"], content: view =>
                {
                    view.Text(["text-xs text-muted-foreground"], "Comp.");
                    view.Text(["text-sm font-semibold"], $"{app.Result.CompletenessScore}%");
                });
                view.Column(["items-center gap-0"], content: view =>
                {
                    view.Text(["text-xs text-muted-foreground"], "Risk");
                    view.Text(["text-sm font-semibold"], $"{app.Result.RiskScore}");
                });
            });

            // Routing badge
            view.Box([Badge.DefaultSm, routingColor, "shrink-0"], content: v => v.Text([], routingLabel));

            // Status badge
            var (statusLabel, statusColor) = app.Status switch
            {
                PermitReady.ApplicationStatus.PermitIssued        => ("Issued ✓",   "bg-success-primary/10 text-success-primary border-success"),
                PermitReady.ApplicationStatus.ReadyForCollection  => ("Ready",       "bg-success-primary/10 text-success-primary border-success"),
                PermitReady.ApplicationStatus.PermitInProduction  => ("Printing",    "bg-blue-500/10 text-blue-600 border-blue-300"),
                PermitReady.ApplicationStatus.AwaitingBiometrics  => ("Biometrics",  "bg-blue-500/10 text-blue-600 border-blue-300"),
                PermitReady.ApplicationStatus.BiometricsComplete  => ("Bio Done",    "bg-blue-500/10 text-blue-600 border-blue-300"),
                PermitReady.ApplicationStatus.Approved            => ("Approved",    "bg-success-primary/10 text-success-primary border-success"),
                PermitReady.ApplicationStatus.Rejected            => ("Rejected",    "bg-error-primary/20 text-error-primary border-error"),
                PermitReady.ApplicationStatus.SupplementRequested => ("Supplement",  "bg-warning-primary/20 text-warning-primary border-warning"),
                PermitReady.ApplicationStatus.SupplementReceived  => ("Responded",   "bg-warning-primary/10 text-warning-primary border-warning"),
                PermitReady.ApplicationStatus.UnderReview         => ("In Review",   "bg-primary/10 text-primary border-primary/30"),
                _                                                 => ("Screened",    "bg-muted text-muted-foreground border-border"),
            };
            view.Box([Badge.DefaultSm, statusColor, "shrink-0 text-[10px]"], content: v => v.Text([], statusLabel));

            // View arrow
            view.Button([Button.GhostSm, Button.Size.Icon],
                content: v => v.Icon([Icon.Default, "w-4 h-4"], name: "chevron-right"),
                onClick: async () =>
                {
                    _selectedAppId.Value = app.ApplicationId;
                    // Clear messages so InitializeChatAsync runs for this application
                    // (it will restore from cache if available)
                    _chatMessages.Value  = [];
                    _chatStreaming.Value  = false;
                    Navigate("detail");
                });
        });
    }

    private static void KpiCard(UIView view, string value, string label, string icon, string color)
    {
        view.Row([Card.Default, "px-4 py-3 items-center gap-3 min-w-[130px]"], content: view =>
        {
            view.Icon([color, "w-5 h-5 shrink-0"], name: icon);
            view.Column(["gap-0"], content: view =>
            {
                view.Text(["text-xl font-bold font-heading " + color], value);
                view.Text(["text-xs text-muted-foreground"], label);
            });
        });
    }
}
