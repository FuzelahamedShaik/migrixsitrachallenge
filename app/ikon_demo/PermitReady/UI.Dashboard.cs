public partial class IkonDemoApp
{
    private void RenderDashboard(UIView view)
    {
        var apps = _applications.Value;

        // Apply advanced filters
        if (!string.IsNullOrEmpty(_filterCategory.Value))
            apps = apps.Where(a => a.Input.Category.ToString() == _filterCategory.Value).ToList();

        if (!string.IsNullOrEmpty(_filterStatus.Value))
            apps = apps.Where(a => a.Status.ToString() == _filterStatus.Value).ToList();

        if (!string.IsNullOrEmpty(_filterRiskLevel.Value))
        {
            apps = apps.Where(a => _filterRiskLevel.Value switch
            {
                "low" => a.Result.RiskScore < 33,
                "medium" => a.Result.RiskScore >= 33 && a.Result.RiskScore < 66,
                "high" => a.Result.RiskScore >= 66,
                _ => true
            }).ToList();
        }

        if (!string.IsNullOrEmpty(_filterCompleteLevel.Value))
        {
            apps = apps.Where(a => _filterCompleteLevel.Value switch
            {
                "low" => a.Result.CompletenessScore < 50,
                "medium" => a.Result.CompletenessScore >= 50 && a.Result.CompletenessScore < 85,
                "high" => a.Result.CompletenessScore >= 85,
                _ => true
            }).ToList();
        }

        var fastTrack    = apps.Where(a => a.Result.Routing == PermitReady.RoutingType.FastTrack).ToList();
        var supplement   = apps.Where(a => a.Result.Routing == PermitReady.RoutingType.SupplementRequested).ToList();
        var specialist   = apps.Where(a => a.Result.Routing == PermitReady.RoutingType.SpecialistReview).ToList();
        var unpaid       = apps.Where(a => a.PaymentStatus == PermitReady.PaymentStatus.Pending).ToList();

        int avgRisk = apps.Count > 0 ? (int)apps.Average(a => a.Result.RiskScore) : 0;
        int avgComp = apps.Count > 0 ? (int)apps.Average(a => a.Result.CompletenessScore) : 0;

        view.Column(["h-full flex flex-col overflow-hidden"], content: view =>
        {
            // Dashboard header — fixed height, never shrinks
            view.Row(["px-8 py-5 border-b border-border justify-between items-center shrink-0"], content: view =>
            {
                view.Column(["gap-0.5"], content: view =>
                {
                    view.Text([Text.H2], "Applications Dashboard");
                    view.Text(["text-sm text-muted-foreground"], $"{apps.Count} total applications · Avg risk {avgRisk}/100 · Avg completeness {avgComp}/100");
                });
            });

            // How-to guide (shown on every dashboard visit until skipped)
            if (!_dashboardGuideDismissed.Value)
                RenderDashboardGuide(view);

            // Filter panel (collapsible)
            if (_showFilters.Value)
                RenderDashboardFilters(view, _applications.Value);

            // KPI cards — fixed height, never shrinks
            view.Row(["px-8 py-4 gap-4 border-b border-border flex-wrap shrink-0"], content: view =>
            {
                view.Button([Button.OutlineSm, "gap-2 shrink-0"],
                    onClick: async () => { _showFilters.Value = !_showFilters.Value; },
                    content: v =>
                    {
                        v.Icon(["w-4 h-4"], name: _showFilters.Value ? "chevron-up" : "sliders");
                        v.Text([], _showFilters.Value ? "Hide Filters" : "Filters");
                    });

                KpiCard(view, apps.Count.ToString(),       "Total",          "layers",       "text-foreground");
                KpiCard(view, fastTrack.Count.ToString(),  "Fast Track",     "zap",          "text-success-primary");
                KpiCard(view, supplement.Count.ToString(), "Supplement",     "alert-circle", "text-warning-primary");
                KpiCard(view, specialist.Count.ToString(), "Specialist",     "user-check",   "text-error-primary");
                if (unpaid.Count > 0)
                    KpiCard(view, unpaid.Count.ToString(), "Awaiting Payment", "clock",      "text-amber-600");
            });

            // Tabs — flex-1 so it fills all remaining vertical space
            view.Box(["px-8 pt-4 flex-1 min-h-0 flex flex-col"], content: view =>
            {
                view.Tabs(
                    value: _dashboardTab.Value,
                    onValueChange: async v => { _dashboardTab.Value = v ?? "all"; },
                    listStyle: [Tabs.List, "shrink-0"],
                    triggerStyle: [Tabs.Trigger],
                    contentStyle: [Tabs.Content, "flex-1 min-h-0"],
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
        view.ScrollArea(rootStyle: ["h-full"], content: view =>
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

            view.Column(["gap-2 py-2 pb-6"], content: view =>
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

        view.Button([Card.Default, "w-full px-4 py-3 flex flex-row items-center gap-4 hover:bg-muted/50 transition-colors text-left"],
            onClick: async () =>
            {
                _pendingAccessAppId.Value     = app.ApplicationId;
                _accessReason.Value           = "";
                _accessReasonError.Value      = "";
                _showAccessReasonDialog.Value = true;
            },
            content: view =>
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

            // Payment badge
            if (app.PaymentStatus == PermitReady.PaymentStatus.Pending)
                view.Box([Badge.DefaultSm, "bg-amber-500/10 text-amber-700 border-amber-300 shrink-0"],
                    content: v => v.Row(["items-center gap-1"], content: v =>
                    {
                        v.Icon(["w-3 h-3"], name: "clock");
                        v.Text([], "Unpaid");
                    }));
            else if (app.FeeAmount > 0)
                view.Box([Badge.DefaultSm, "bg-success-primary/10 text-success-primary border-success shrink-0"],
                    content: v => v.Row(["items-center gap-1"], content: v =>
                    {
                        v.Icon(["w-3 h-3"], name: "check");
                        v.Text([], $"€{app.FeeAmount:N0} paid");
                    }));

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

            // Unread applicant reply badge
            var unread = UnreadApplicantReplies(app.ApplicationId);
            if (unread > 0)
            {
                view.Box(["min-w-[20px] h-5 rounded-full bg-error-primary flex items-center justify-center px-1 shrink-0"],
                    content: v => v.Text(["text-[10px] text-white font-bold"], unread.ToString()));
            }

            // Visual affordance — non-interactive indicator
            view.Icon(["w-4 h-4 text-muted-foreground shrink-0"], name: "chevron-right");
        });
    }

    private void RenderDashboardGuide(UIView view)
    {
        var steps = new[]
        {
            (
                icon:  "mouse-pointer-click",
                color: "text-primary",
                bg:    "bg-primary/10",
                num:   "1",
                title: "Open an Application",
                desc:  "Click any row in the list below to open the applicant's full file. You must state a reason for access (GDPR compliance)."
            ),
            (
                icon:  "bot",
                color: "text-violet-600",
                bg:    "bg-violet-500/10",
                num:   "2",
                title: "AI Analysis",
                desc:  "The AI instantly screens the application — completeness score, risk flags, missing documents — and answers your questions."
            ),
            (
                icon:  "mail",
                color: "text-amber-600",
                bg:    "bg-amber-500/10",
                num:   "3",
                title: "Communicate via Messages",
                desc:  "Use the Messages tab to send supplement requests or general messages. The applicant sees them in real time on their tracking page."
            ),
            (
                icon:  "gavel",
                color: "text-emerald-600",
                bg:    "bg-emerald-500/10",
                num:   "4",
                title: "Make a Decision",
                desc:  "Approve, reject, or request biometrics from the Decision panel. Status changes are reflected immediately in the applicant's portal."
            ),
        };

        view.Box(["px-8 py-4 border-b border-border bg-muted/30"], content: view =>
        {
            view.Column(["gap-3"], content: view =>
            {
                // Header row
                view.Row(["items-center justify-between"], content: view =>
                {
                    view.Row(["items-center gap-2"], content: view =>
                    {
                        view.Icon(["w-4 h-4 text-primary"], name: "info");
                        view.Text(["text-sm font-semibold"], "How to use PermitReady");
                        view.Box(["bg-primary/10 text-primary text-[10px] font-semibold px-2 py-0.5 rounded-full"],
                            content: v => v.Text([], "Quick Guide"));
                    });
                    view.Button([Button.GhostSm, "text-xs text-muted-foreground gap-1"],
                        content: v =>
                        {
                            v.Text([], "Skip");
                            v.Icon(["w-3.5 h-3.5"], name: "x");
                        },
                        onClick: async () => { _dashboardGuideDismissed.Value = true; });
                });

                // Step cards
                view.Row(["gap-3"], content: view =>
                {
                    foreach (var step in steps)
                    {
                        view.Column(["flex-1 bg-background rounded-xl border border-border px-4 py-3 gap-2"], content: view =>
                        {
                            view.Row(["items-center gap-2"], content: view =>
                            {
                                view.Box([$"w-7 h-7 rounded-lg {step.bg} flex items-center justify-center shrink-0"],
                                    content: v => v.Icon([$"{step.color} w-4 h-4"], name: step.icon));
                                view.Box(["w-5 h-5 rounded-full bg-muted flex items-center justify-center shrink-0"],
                                    content: v => v.Text(["text-[10px] font-bold text-muted-foreground"], step.num));
                                view.Text(["text-xs font-semibold flex-1"], step.title);
                            });
                            view.Text(["text-[11px] text-muted-foreground leading-relaxed"], step.desc);
                        });
                    }
                });
            });
        });
    }

    private void RenderDashboardFilters(UIView view, List<PermitReady.StoredApplication> allApps)
    {
        view.Box(["px-8 py-4 bg-muted/30 border-b border-border gap-4"], content: view =>
        {
            view.Column(["gap-4"], content: view =>
            {
                view.Text(["text-sm font-medium"], "Filters");

                view.Row(["gap-4 flex-wrap"], content: view =>
                {
                    // Category filter
                    view.Column(["gap-1.5"], content: view =>
                    {
                        view.Text(["text-xs font-medium"], "Permit Category");
                        var uniqueCategories = allApps.Select(a => a.Input.Category).Distinct().OrderBy(c => c.DisplayName()).ToList();
                        view.Row(["gap-2 flex-wrap"], content: view =>
                        {
                            var isAll = string.IsNullOrEmpty(_filterCategory.Value);
                            view.Button([isAll ? Button.PrimarySm : Button.OutlineSm],
                                onClick: async () => { _filterCategory.Value = ""; },
                                content: v => v.Text([], "All"));
                            foreach (var cat in uniqueCategories.Take(8))
                            {
                                var c = cat;
                                var isSelected = _filterCategory.Value == c.ToString();
                                view.Button([isSelected ? Button.PrimarySm : Button.OutlineSm],
                                    onClick: async () => { _filterCategory.Value = isSelected ? "" : c.ToString(); },
                                    content: v => v.Text([], c.ShortName()));
                            }
                        });
                    });

                    // Risk level filter
                    view.Column(["gap-1.5"], content: view =>
                    {
                        view.Text(["text-xs font-medium"], "Risk Level");
                        view.Row(["gap-2"], content: view =>
                        {
                            FilterButton(view, "All", "", _filterRiskLevel.Value);
                            FilterButton(view, "Low", "low", _filterRiskLevel.Value);
                            FilterButton(view, "Medium", "medium", _filterRiskLevel.Value);
                            FilterButton(view, "High", "high", _filterRiskLevel.Value);
                        });
                    });

                    // Completeness filter
                    view.Column(["gap-1.5"], content: view =>
                    {
                        view.Text(["text-xs font-medium"], "Completeness");
                        view.Row(["gap-2"], content: view =>
                        {
                            FilterButton(view, "All", "", _filterCompleteLevel.Value);
                            FilterButton(view, "Low (<50%)", "low", _filterCompleteLevel.Value);
                            FilterButton(view, "Medium (50-85%)", "medium", _filterCompleteLevel.Value);
                            FilterButton(view, "High (85%+)", "high", _filterCompleteLevel.Value);
                        });
                    });

                    // Clear filters button
                    view.Button([Button.OutlineSm, "gap-1"],
                        onClick: async () =>
                        {
                            _filterCategory.Value = "";
                            _filterStatus.Value = "";
                            _filterRiskLevel.Value = "";
                            _filterCompleteLevel.Value = "";
                        },
                        content: v =>
                        {
                            v.Icon(["w-4 h-4"], name: "x");
                            v.Text([], "Clear All");
                        });
                });
            });
        });
    }

    private void FilterButton(UIView view, string label, string value, string currentValue)
    {
        var isSelected = currentValue == value;
        var isCompleteness = label.Contains("<") || label.Contains("%");

        view.Button([isSelected && !string.IsNullOrEmpty(value) ? Button.PrimarySm : Button.OutlineSm],
            onClick: async () =>
            {
                if (isCompleteness)
                    _filterCompleteLevel.Value = isSelected ? "" : value;
                else
                    _filterRiskLevel.Value = isSelected ? "" : value;
            },
            content: v => v.Text([], label));
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
