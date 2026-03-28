public partial class IkonDemoApp
{
    private void RenderLanding(UIView view)
    {
        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column([Container.Md, Layout.Center, "py-16 px-4 min-h-full"], content: view =>
            {
                // Hero
                view.Column(["items-center text-center gap-4 mb-12"], content: view =>
                {
                    view.Box(["w-16 h-16 rounded-2xl bg-primary flex items-center justify-center mb-2"], content: view =>
                        view.Text(["text-primary-foreground text-2xl font-bold"], "PR"));

                    view.Text(["text-4xl font-bold tracking-tight font-heading"], "PermitReady");
                    view.Text(["text-lg text-muted-foreground max-w-md"],
                        "AI-powered residence permit completeness checker for faster, fairer processing.");
                });

                // Role cards
                view.Row(["gap-6 w-full justify-center flex-wrap"], content: view =>
                {
                    // Applicant card
                    view.Column([Card.Default, "p-8 flex-1 min-w-[260px] max-w-[340px] cursor-pointer hover:shadow-lg transition-shadow items-center text-center gap-4"], content: view =>
                    {
                        view.Box(["w-14 h-14 rounded-full bg-blue-100 flex items-center justify-center"], content: view =>
                            view.Icon(["text-blue-600 w-7 h-7"], name: "user"));

                        view.Column(["gap-1 items-center"], content: view =>
                        {
                            view.Text([Text.H3], "I'm applying");
                            view.Text(["text-sm text-muted-foreground text-center"],
                                "Submit your application and instantly check how complete it is.");
                        });

                        view.Button([Button.PrimaryMd, "w-full mt-2"], "Get Started",
                            onClick: async () =>
                            {
                                _role.Value = "applicant";
                                Navigate("applicant_home");
                            });
                    });

                    // Officer card
                    view.Column([Card.Default, "p-8 flex-1 min-w-[260px] max-w-[340px] items-center text-center gap-4"], content: view =>
                    {
                        view.Box(["w-14 h-14 rounded-full bg-purple-100 flex items-center justify-center"], content: view =>
                            view.Icon(["text-purple-600 w-7 h-7"], name: "shield"));

                        view.Column(["gap-1 items-center"], content: view =>
                        {
                            view.Text([Text.H3], "Migri Officer");
                            view.Text(["text-sm text-muted-foreground text-center"],
                                "Access the officer dashboard to review and triage applications.");
                        });

                        // PIN field + button
                        view.Column(["gap-2 w-full mt-2"], content: view =>
                        {
                            view.TextField([Input.Default, "text-center tracking-[0.4em] font-mono"],
                                placeholder: "Enter PIN",
                                value: _officerPin.Value,
                                onValueChange: async v =>
                                {
                                    _officerPin.Value = v;
                                    _pinError.Value   = "";
                                });

                            if (!string.IsNullOrEmpty(_pinError.Value))
                                view.Text(["text-xs text-error-primary text-center"], _pinError.Value);

                            view.Button([Button.SecondaryMd, "w-full"], "Access Dashboard",
                                onClick: async () => VerifyOfficerPin());
                        });
                    });
                });

                // Footer note
                view.Text(["text-xs text-muted-foreground text-center mt-8"],
                    "Demo: Officer PIN is 1234  ·  6 seed applications pre-loaded");
            });
        });
    }
}
