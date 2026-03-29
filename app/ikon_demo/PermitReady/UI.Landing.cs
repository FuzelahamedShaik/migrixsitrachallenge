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
                        });

                        // Right: Video Guides
                        view.Column(["flex-1 min-w-[300px] gap-4"], content: view =>
                        {
                            view.Column(["gap-2"], content: view =>
                            {
                                view.Text(["text-lg font-semibold"], "Official Migri Guides");
                                view.Text(["text-sm text-neutral-600"],
                                    "Learn from Finnish Immigration Service videos");
                            });

                            // Video cards
                            view.Column(["gap-2.5"], content: view =>
                            {
                                // Video 1: Remember student permit requirements
                                view.Button([Button.GhostMd, "w-full p-3 border border-neutral-200 rounded-lg text-left hover:bg-neutral-50 hover:border-blue-300 flex items-start gap-3 h-auto justify-start transition-all"],
                                    href: "https://youtu.be/3X2cVhux4RA",
                                    target: "_blank",
                                    content: view =>
                                    {
                                        view.Box(["w-10 h-10 rounded-md bg-blue-100 flex items-center justify-center flex-shrink-0"], content: view =>
                                            view.Icon(["text-blue-600 w-5 h-5"], name: "play"));

                                        view.Column(["flex-1 gap-1 min-w-0"], content: view =>
                                        {
                                            view.Text(["text-sm font-semibold line-clamp-2"],
                                                "Student Permit Requirements");
                                            view.Text(["text-xs text-neutral-500"],
                                                "Remember these when applying for studies");
                                        });
                                    });

                                // Video 2: How to select the right work permit
                                view.Button([Button.GhostMd, "w-full p-3 border border-neutral-200 rounded-lg text-left hover:bg-neutral-50 hover:border-blue-300 flex items-start gap-3 h-auto justify-start transition-all"],
                                    href: "https://youtu.be/5rWtsCMcNaQ",
                                    target: "_blank",
                                    content: view =>
                                    {
                                        view.Box(["w-10 h-10 rounded-md bg-emerald-100 flex items-center justify-center flex-shrink-0"], content: view =>
                                            view.Icon(["text-emerald-600 w-5 h-5"], name: "play"));

                                        view.Column(["flex-1 gap-1 min-w-0"], content: view =>
                                        {
                                            view.Text(["text-sm font-semibold line-clamp-2"],
                                                "Right Work Permit Type");
                                            view.Text(["text-xs text-neutral-500"],
                                                "How to select the right employment permit");
                                        });
                                    });

                                // Video 3: After graduation
                                view.Button([Button.GhostMd, "w-full p-3 border border-neutral-200 rounded-lg text-left hover:bg-neutral-50 hover:border-blue-300 flex items-start gap-3 h-auto justify-start transition-all"],
                                    href: "https://youtu.be/Y1ciw3vmhiA",
                                    target: "_blank",
                                    content: view =>
                                    {
                                        view.Box(["w-10 h-10 rounded-md bg-amber-100 flex items-center justify-center flex-shrink-0"], content: view =>
                                            view.Icon(["text-amber-600 w-5 h-5"], name: "play"));

                                        view.Column(["flex-1 gap-1 min-w-0"], content: view =>
                                        {
                                            view.Text(["text-sm font-semibold line-clamp-2"],
                                                "After Graduation");
                                            view.Text(["text-xs text-neutral-500"],
                                                "How to work in Finland after your studies");
                                        });
                                    });

                                // Video 4: Fast-track entrepreneur
                                view.Button([Button.GhostMd, "w-full p-3 border border-neutral-200 rounded-lg text-left hover:bg-neutral-50 hover:border-blue-300 flex items-start gap-3 h-auto justify-start transition-all"],
                                    href: "https://youtu.be/PTlU4OOPeXE",
                                    target: "_blank",
                                    content: view =>
                                    {
                                        view.Box(["w-10 h-10 rounded-md bg-purple-100 flex items-center justify-center flex-shrink-0"], content: view =>
                                            view.Icon(["text-purple-600 w-5 h-5"], name: "play"));

                                        view.Column(["flex-1 gap-1 min-w-0"], content: view =>
                                        {
                                            view.Text(["text-sm font-semibold line-clamp-2"],
                                                "Startup Entrepreneur");
                                            view.Text(["text-xs text-neutral-500"],
                                                "Fast-track startup permit application");
                                        });
                                    });
                            });

                            // Tip box
                            view.Column(["gap-2 mt-2 p-3 bg-amber-50 rounded-lg border border-amber-200"], content: view =>
                            {
                                view.Row(["gap-2 items-start"], content: view =>
                                {
                                    view.Icon(["w-4 h-4 text-amber-600 flex-shrink-0 mt-0.5"], name: "lightbulb");
                                    view.Text(["text-xs text-amber-800"],
                                        "All videos from the official Finnish Immigration Service (Maahanmuuttovirasto)");
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
