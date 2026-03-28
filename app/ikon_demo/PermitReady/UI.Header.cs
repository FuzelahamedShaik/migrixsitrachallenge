public partial class IkonDemoApp
{
    private record Crumb(string Label, string? TargetPage); // null = current page (not clickable)

    private void RenderHeader(UIView view)
    {
        var crumbs = BuildCrumbs(_page.Value);

        view.Column(["border-b border-border bg-background"], content: view =>
        {
            // ── Top bar ───────────────────────────────────────────────────
            view.Row(["px-6 h-14 items-center justify-between gap-4"], content: view =>
            {
                // Left: logo (always links to /)
                view.Button([Button.GhostSm, "flex items-center gap-2.5 -ml-2 hover:bg-transparent px-2"],
                    onClick: async () => Navigate("landing"),
                    content: view =>
                    {
                        view.Box(["w-7 h-7 rounded-md bg-primary flex items-center justify-center shrink-0"], content: view =>
                            view.Text(["text-primary-foreground text-xs font-bold"], "PR"));
                        view.Text(["text-sm font-bold tracking-tight font-heading"], "PermitReady");
                    });

                // Right: breadcrumbs + role badge + theme toggle
                view.Row(["items-center gap-4"], content: view =>
                {
                    // Breadcrumb trail (inline, right-aligned)
                    if (crumbs.Length > 0)
                    {
                        view.Row([Breadcrumb.Root], content: view =>
                        {
                            view.Row([Breadcrumb.List], content: view =>
                            {
                                for (int i = 0; i < crumbs.Length; i++)
                                {
                                    var crumb = crumbs[i];
                                    bool isLast = i == crumbs.Length - 1;

                                    view.Box([Breadcrumb.Item], content: view =>
                                    {
                                        if (!isLast && crumb.TargetPage != null)
                                        {
                                            var target = crumb.TargetPage;
                                            view.Button([Breadcrumb.Link, "text-sm bg-transparent border-0 p-0 h-auto cursor-pointer"],
                                                crumb.Label,
                                                onClick: async () => Navigate(target));
                                        }
                                        else
                                        {
                                            view.Text([Breadcrumb.Page, "text-sm"], crumb.Label);
                                        }
                                    });

                                    if (!isLast)
                                        view.Text([Breadcrumb.Separator, "text-sm select-none"], "/");
                                }
                            });
                        });
                    }

                    // Role badge
                    switch (_role.Value)
                    {
                        case "applicant":
                            view.Box([Badge.DefaultSm, "bg-blue-100 text-blue-700 border-blue-200"], content: v =>
                                v.Text([], "Applicant"));
                            break;
                        case "officer":
                            view.Box([Badge.DefaultSm, "bg-purple-100 text-purple-700 border-purple-200"], content: v =>
                                v.Text([], "Migri Officer"));
                            break;
                    }

                    // Theme toggle
                    view.Button([Button.GhostMd, Button.Size.Icon],
                        onClick: ToggleThemeAsync,
                        content: v => v.Icon([Icon.Default],
                            name: _theme.Value == Constants.DarkTheme ? "sun" : "moon"));
                });
            });
        });
    }

    // ── Breadcrumb trail per page ─────────────────────────────────────────

    private Crumb[] BuildCrumbs(string page) => page switch
    {
        "landing"        => [],

        "applicant_home" =>
        [
            new("Home", null),
        ],

        "form" =>
        [
            new("Home",            "applicant_home"),
            new("Apply",           null),
        ],

        "review" =>
        [
            new("Home",            "applicant_home"),
            new("Apply",           "form"),
            new("Review",          null),
        ],

        "results" =>
        [
            new("Home",            "applicant_home"),
            new("Apply",           "form"),
            new("Results",         null),
        ],

        "dashboard" =>
        [
            new("Home",            "landing"),
            new("Dashboard",       null),
        ],

        "detail" =>
        [
            new("Home",            "landing"),
            new("Dashboard",       "dashboard"),
            new(string.IsNullOrEmpty(_selectedAppId.Value)
                    ? "Application"
                    : $"Application {_selectedAppId.Value}",
                null),
        ],

        _ => [],
    };
}
