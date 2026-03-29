public partial class IkonDemoApp
{
    private void RenderLanding(UIView view)
    {
        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column(["w-full min-h-full"], content: view =>
            {
                // ── Hero Section ──────────────────────────────────────────────
                view.Box(["w-full bg-gradient-to-b from-[#1F3A5F] to-[#2F5A8E] text-white py-12 px-4"], content: view =>
                {
                    view.Column([Container.Lg, Layout.Center, "gap-4 text-center"], content: view =>
                    {
                        view.Text(["text-4xl font-bold font-heading tracking-tight"],
                            "Smart Immigration. Finnish Standards.");

                        view.Text(["text-lg text-white/90"],
                            "AI-powered application assistance integrated into Migri's official system.");
                    });
                });

                // ── Two Column Section: Role Selection + Video ──────────────────
                view.Box(["w-full bg-white px-4 py-12"], content: view =>
                {
                    view.Row([Container.Lg, Layout.Center, "gap-8 flex-wrap lg:flex-nowrap items-start"], content: view =>
                    {
                        // Left: Role Selection (Applicant)
                        view.Column(["flex-1 min-w-[300px]"], content: view =>
                        {
                            view.Column(["gap-3 mb-6"], content: view =>
                            {
                                view.Text(["text-2xl font-bold"], "I'm applying for a residence permit");
                                view.Text(["text-neutral-600"],
                                    "Complete your application with AI-powered guidance and real-time document verification.");
                            });

                            // Applicant card
                            view.Column([Card.Default, "p-7 border-2 border-blue-300 gap-6 cursor-pointer hover:shadow-lg transition-all"], content: view =>
                            {
                                view.Column(["gap-4"], content: view =>
                                {
                                    view.Box(["w-14 h-14 rounded-full bg-blue-100 flex items-center justify-center"], content: view =>
                                        view.Icon(["text-blue-600 w-7 h-7"], name: "user-check"));

                                    view.Column(["gap-2"], content: view =>
                                    {
                                        view.Text(["text-xl font-bold"], "Start Your Application");
                                        view.Text(["text-sm text-neutral-600"],
                                            "Apply for work, study, family, or other residence permits.");
                                    });
                                });

                                // Benefits list
                                view.Column(["gap-3 py-4 border-y border-neutral-200"], content: view =>
                                {
                                    view.Row(["gap-3 items-start"], content: view =>
                                    {
                                        view.Icon(["w-5 h-5 text-emerald-600 flex-shrink-0 mt-0.5"], name: "file-check");
                                        view.Text(["text-sm text-neutral-700"], "Smart document checklist");
                                    });

                                    view.Row(["gap-3 items-start"], content: view =>
                                    {
                                        view.Icon(["w-5 h-5 text-emerald-600 flex-shrink-0 mt-0.5"], name: "brain");
                                        view.Text(["text-sm text-neutral-700"], "AI-powered guidance");
                                    });

                                    view.Row(["gap-3 items-start"], content: view =>
                                    {
                                        view.Icon(["w-5 h-5 text-emerald-600 flex-shrink-0 mt-0.5"], name: "zap");
                                        view.Text(["text-sm text-neutral-700"], "Completeness scoring");
                                    });
                                });

                                view.Button([Button.PrimaryMd, "w-full px-6 py-3 text-base"], "Get Started",
                                    onClick: async () =>
                                    {
                                        _role.Value = "applicant";
                                        Navigate("applicant_home");
                                    });
                            });

                            // Officer login (subtle)
                            view.Column(["mt-8 p-4 bg-neutral-50 rounded-lg border border-neutral-200 gap-3"], content: view =>
                            {
                                view.Text(["text-sm font-semibold"], "Migri Officer?");
                                view.Text(["text-xs text-neutral-600 mb-2"],
                                    "Access the officer dashboard to review applications.");

                                view.TextField([Input.Default, "text-center tracking-[0.3em] font-mono text-sm"],
                                    placeholder: "PIN",
                                    value: _officerPin.Value,
                                    onValueChange: async v =>
                                    {
                                        _officerPin.Value = v;
                                        _pinError.Value   = "";
                                    });

                                if (!string.IsNullOrEmpty(_pinError.Value))
                                    view.Text(["text-xs text-rose-600 font-semibold"], _pinError.Value);

                                view.Button([Button.SecondaryMd, "w-full py-2 text-sm"], "Access Dashboard",
                                    onClick: async () => VerifyOfficerPin());
                            });
                        });

                        // Right: YouTube Video
                        view.Column(["flex-1 min-w-[300px] gap-3"], content: view =>
                        {
                            view.Column(["gap-2 mb-2"], content: view =>
                            {
                                view.Text(["text-lg font-semibold"], "How to Apply");
                                view.Text(["text-sm text-neutral-600"],
                                    "Watch this short guide to understand the application process.");
                            });

                            // Video embed container
                            view.Column(["w-full bg-neutral-900 rounded-lg overflow-hidden aspect-video flex items-center justify-center cursor-pointer hover:bg-black transition-colors group"], content: view =>
                            {
                                view.Column(["gap-3 items-center"], content: view =>
                                {
                                    view.Box(["w-16 h-16 rounded-full bg-white/20 flex items-center justify-center group-hover:bg-white/30 transition-colors"], content: view =>
                                        view.Icon(["w-8 h-8 text-white ml-1"], name: "play"));

                                    view.Column(["text-center gap-1"], content: view =>
                                    {
                                        view.Text(["text-white font-semibold"], "Watch Guide Video");
                                        view.Text(["text-white/60 text-sm"], "How to Complete Your Application");
                                    });
                                });
                            });

                            // Additional info
                            view.Column(["gap-2 mt-4 p-4 bg-amber-50 rounded-lg border border-amber-200"], content: view =>
                            {
                                view.Row(["gap-2 items-start"], content: view =>
                                {
                                    view.Icon(["w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5"], name: "lightbulb");
                                    view.Column(["gap-1"], content: view =>
                                    {
                                        view.Text(["text-sm font-semibold text-amber-900"], "Pro Tip");
                                        view.Text(["text-xs text-amber-800"],
                                            "Prepare your documents before starting. The application takes about 10 minutes with AI assistance.");
                                    });
                                });
                            });
                        });
                    });
                });

                // ── Footer ────────────────────────────────────────────────────
                view.Box(["w-full bg-neutral-50 border-t border-neutral-200 py-6 px-4"], content: view =>
                {
                    view.Column([Container.Lg, Layout.Center, "text-center gap-2"], content: view =>
                    {
                        view.Text(["text-xs text-neutral-600"],
                            "This system is integrated with Migri's official residence permit application process.");
                        view.Text(["text-xs text-neutral-500"],
                            "Demo: Officer PIN is 1234");
                    });
                });
            });
        });
    }
}
