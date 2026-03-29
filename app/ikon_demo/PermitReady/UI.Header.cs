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

                // Right: breadcrumbs + role badge + theme toggle + officer login
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

                    // Officer controls (visible on landing page only)
                    if (_page.Value == "landing")
                    {
                        if (string.IsNullOrEmpty(_role.Value) || _role.Value == "applicant")
                        {
                            // Show login form if not logged in or applicant
                            view.Row(["gap-2 items-center"], content: view =>
                            {
                                view.Text(["text-xs text-neutral-500"], "If officer:");
                                view.TextField([Input.Default, "w-24 h-8 text-xs tracking-[0.2em] font-mono"],
                                    placeholder: "PIN",
                                    value: _officerPin.Value,
                                    onValueChange: async v =>
                                    {
                                        _officerPin.Value = v;
                                        _pinError.Value   = "";
                                    });

                                view.Button([Button.GhostMd, "px-3 h-8 text-xs"],
                                    "Login",
                                    onClick: async () => VerifyOfficerPin());

                                if (!string.IsNullOrEmpty(_pinError.Value))
                                    view.Box(["absolute right-6 top-20 bg-red-50 border border-red-200 rounded-md px-3 py-2"], content: view =>
                                        view.Text(["text-xs text-rose-600"], _pinError.Value));
                            });
                        }
                        else if (_role.Value == "officer")
                        {
                            // Show dashboard link if logged in as officer
                            view.Button([Button.PrimarySm, "px-4 h-8 text-xs"],
                                "Go to Dashboard",
                                onClick: async () => Navigate("dashboard"));
                        }
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
