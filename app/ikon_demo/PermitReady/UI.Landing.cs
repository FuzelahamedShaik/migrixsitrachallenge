public partial class IkonDemoApp
{
    private void RenderLanding(UIView view)
    {
        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column(["w-full min-h-full"], content: view =>
            {
                // ── Hero Section ──────────────────────────────────────────────
                view.Box(["w-full bg-gradient-to-b from-[#1F3A5F] to-[#2F5A8E] text-white py-16 px-4"], content: view =>
                {
                    view.Column([Container.Md, Layout.Center, "gap-6 text-center"], content: view =>
                    {
                        view.Text(["text-5xl font-bold font-heading tracking-tight leading-tight"],
                            "Smart Immigration.\nFinnish Standards.");

                        view.Text(["text-xl text-white/90 max-w-2xl"],
                            "AI-powered application assistance integrated into Migri's official system. Get real-time guidance, verify your documents, and submit with confidence.");

                        // Hero CTA buttons
                        view.Row(["gap-4 justify-center pt-4 flex-wrap"], content: view =>
                        {
                            view.Button([Button.PrimaryMd, "px-8 py-3 bg-white text-[#1F3A5F] hover:bg-gray-100 font-semibold"],
                                "Start Your Application",
                                onClick: async () =>
                                {
                                    _role.Value = "applicant";
                                    Navigate("applicant_home");
                                });

                            view.Button([Button.SecondaryMd, "px-8 py-3 border-2 border-white text-white hover:bg-white/10 font-semibold"],
                                "Learn More",
                                onClick: async () => { /* scroll to features */ });
                        });

                        // Trust indicators
                        view.Row(["gap-6 justify-center mt-8 text-sm text-white/80 flex-wrap"], content: view =>
                        {
                            view.Row(["gap-2 items-center"], content: view =>
                            {
                                view.Icon(["w-5 h-5"], name: "check-circle");
                                view.Text([], "Official Migri Integration");
                            });

                            view.Row(["gap-2 items-center"], content: view =>
                            {
                                view.Icon(["w-5 h-5"], name: "zap");
                                view.Text([], "Real-Time Verification");
                            });

                            view.Row(["gap-2 items-center"], content: view =>
                            {
                                view.Icon(["w-5 h-5"], name: "shield");
                                view.Text([], "Secure & Compliant");
                            });
                        });
                    });
                });

                // ── Benefits Section ──────────────────────────────────────────
                view.Box(["w-full bg-white py-14 px-4"], content: view =>
                {
                    view.Column([Container.Md, Layout.Center, "gap-12"], content: view =>
                    {
                        view.Column(["text-center gap-2 mb-4"], content: view =>
                        {
                            view.Text(["text-3xl font-bold"], "Why Choose Migri's AI Assistant");
                            view.Text(["text-neutral-600"], "Streamlined application process with intelligent guidance");
                        });

                        // Benefits grid
                        view.Row(["gap-8 flex-wrap justify-center"], content: view =>
                        {
                            // Benefit 1
                            view.Column(["flex-1 min-w-[260px] gap-3"], content: view =>
                            {
                                view.Box(["w-12 h-12 rounded-lg bg-emerald-100 flex items-center justify-center"], content: view =>
                                    view.Icon(["text-emerald-600 w-6 h-6"], name: "file-check"));

                                view.Text(["font-semibold text-lg"], "Document Verification");
                                view.Text(["text-neutral-600 text-sm"],
                                    "Get instant feedback on missing documents before submission. Avoid delays and rejections.");
                            });

                            // Benefit 2
                            view.Column(["flex-1 min-w-[260px] gap-3"], content: view =>
                            {
                                view.Box(["w-12 h-12 rounded-lg bg-blue-100 flex items-center justify-center"], content: view =>
                                    view.Icon(["text-blue-600 w-6 h-6"], name: "brain"));

                                view.Text(["font-semibold text-lg"], "Intelligent Guidance");
                                view.Text(["text-neutral-600 text-sm"],
                                    "AI-powered assistant understands your unique situation and guides you through each step.");
                            });

                            // Benefit 3
                            view.Column(["flex-1 min-w-[260px] gap-3"], content: view =>
                            {
                                view.Box(["w-12 h-12 rounded-lg bg-amber-100 flex items-center justify-center"], content: view =>
                                    view.Icon(["text-amber-600 w-6 h-6"], name: "clock"));

                                view.Text(["font-semibold text-lg"], "Save Time");
                                view.Text(["text-neutral-600 text-sm"],
                                    "Complete your application in 10 minutes with intelligent form assistance and smart suggestions.");
                            });
                        });
                    });
                });

                // ── Role Selection Cards ──────────────────────────────────────
                view.Box(["w-full bg-neutral-50 py-14 px-4"], content: view =>
                {
                    view.Column([Container.Md, Layout.Center, "gap-12"], content: view =>
                    {
                        view.Column(["text-center gap-2"], content: view =>
                        {
                            view.Text(["text-3xl font-bold"], "Select Your Role");
                            view.Text(["text-neutral-600"], "Choose how you want to proceed");
                        });

                        view.Row(["gap-6 w-full justify-center flex-wrap"], content: view =>
                        {
                            // Applicant card
                            view.Column([Card.Default, "p-7 flex-1 min-w-[280px] max-w-[380px] cursor-pointer hover:shadow-xl transition-all hover:border-blue-300 border-2 border-neutral-200 items-center text-center gap-5"], content: view =>
                            {
                                view.Box(["w-16 h-16 rounded-full bg-blue-100 flex items-center justify-center"], content: view =>
                                    view.Icon(["text-blue-600 w-8 h-8"], name: "user-check"));

                                view.Column(["gap-2 items-center"], content: view =>
                                {
                                    view.Text(["text-2xl font-bold"], "I'm Applying");
                                    view.Text(["text-sm text-neutral-600 text-center"],
                                        "Apply for your residence permit with real-time AI-powered verification.");
                                });

                                view.Button([Button.PrimaryMd, "w-full mt-2 px-6 py-3"], "Get Started Now",
                                    onClick: async () =>
                                    {
                                        _role.Value = "applicant";
                                        Navigate("applicant_home");
                                    });

                                // Quick features
                                view.Column(["text-left gap-2 mt-4 pt-4 border-t border-neutral-200 w-full text-xs text-neutral-600"], content: view =>
                                {
                                    view.Row(["gap-2 items-start"], content: view =>
                                    {
                                        view.Icon(["w-4 h-4 text-emerald-600 mt-0.5 flex-shrink-0"], name: "check");
                                        view.Text([], "Document checklist");
                                    });

                                    view.Row(["gap-2 items-start"], content: view =>
                                    {
                                        view.Icon(["w-4 h-4 text-emerald-600 mt-0.5 flex-shrink-0"], name: "check");
                                        view.Text([], "Category-specific guidance");
                                    });

                                    view.Row(["gap-2 items-start"], content: view =>
                                    {
                                        view.Icon(["w-4 h-4 text-emerald-600 mt-0.5 flex-shrink-0"], name: "check");
                                        view.Text([], "Completeness scoring");
                                    });
                                });
                            });

                            // Officer card
                            view.Column([Card.Default, "p-7 flex-1 min-w-[280px] max-w-[380px] border-2 border-neutral-200 items-center text-center gap-5"], content: view =>
                            {
                                view.Box(["w-16 h-16 rounded-full bg-purple-100 flex items-center justify-center"], content: view =>
                                    view.Icon(["text-purple-600 w-8 h-8"], name: "shield-alert"));

                                view.Column(["gap-2 items-center"], content: view =>
                                {
                                    view.Text(["text-2xl font-bold"], "Migri Officer");
                                    view.Text(["text-sm text-neutral-600 text-center"],
                                        "Review and triage applications with integrated AI insights.");
                                });

                                // PIN field + button
                                view.Column(["gap-3 w-full mt-2"], content: view =>
                                {
                                    view.TextField([Input.Default, "text-center tracking-[0.4em] font-mono text-lg"],
                                        placeholder: "Enter PIN",
                                        value: _officerPin.Value,
                                        onValueChange: async v =>
                                        {
                                            _officerPin.Value = v;
                                            _pinError.Value   = "";
                                        });

                                    if (!string.IsNullOrEmpty(_pinError.Value))
                                        view.Text(["text-xs text-rose-600 font-semibold"], _pinError.Value);

                                    view.Button([Button.SecondaryMd, "w-full px-6 py-3"], "Access Dashboard",
                                        onClick: async () => VerifyOfficerPin());
                                });

                                // Quick features
                                view.Column(["text-left gap-2 mt-4 pt-4 border-t border-neutral-200 w-full text-xs text-neutral-600"], content: view =>
                                {
                                    view.Row(["gap-2 items-start"], content: view =>
                                    {
                                        view.Icon(["w-4 h-4 text-purple-600 mt-0.5 flex-shrink-0"], name: "check");
                                        view.Text([], "Application dashboard");
                                    });

                                    view.Row(["gap-2 items-start"], content: view =>
                                    {
                                        view.Icon(["w-4 h-4 text-purple-600 mt-0.5 flex-shrink-0"], name: "check");
                                        view.Text([], "AI-powered scoring");
                                    });

                                    view.Row(["gap-2 items-start"], content: view =>
                                    {
                                        view.Icon(["w-4 h-4 text-purple-600 mt-0.5 flex-shrink-0"], name: "check");
                                        view.Text([], "Secure communication");
                                    });
                                });
                            });
                        });
                    });
                });

                // ── Footer ────────────────────────────────────────────────────
                view.Box(["w-full bg-white border-t border-neutral-200 py-6 px-4"], content: view =>
                {
                    view.Column([Container.Md, Layout.Center, "text-center gap-2"], content: view =>
                    {
                        view.Text(["text-xs text-neutral-500"],
                            "This system is integrated with Migri's official residence permit application process.");
                        view.Text(["text-xs text-neutral-400"],
                            "Demo: Officer PIN is 1234  ·  6 seed applications pre-loaded");
                    });
                });
            });
        });
    }
}
