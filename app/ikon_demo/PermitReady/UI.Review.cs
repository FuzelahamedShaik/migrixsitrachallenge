public partial class IkonDemoApp
{
    private void RenderReview(UIView view)
    {
        var result = _lastResult.Value;
        var input  = _lastInput.Value;

        if (result == null || input == null)
        {
            Navigate("form");
            return;
        }

        bool hasMissing   = result.MissingItems.Count > 0;
        bool hasRiskFlags = result.RiskFlags.Count > 0;

        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column([Container.Md, "py-10 px-4 gap-6 min-h-full"], content: view =>
            {
                // ── Journey stepper ───────────────────────────────────────
                RenderReviewStepper(view);

                // ── Page header ───────────────────────────────────────────
                view.Row(["items-center gap-3"], content: view =>
                {
                    view.Box(["w-10 h-10 rounded-full bg-warning-primary/20 flex items-center justify-center shrink-0"], content: view =>
                        view.Icon(["text-warning-primary w-5 h-5"], name: "alert-triangle"));

                    view.Column(["gap-0.5"], content: view =>
                    {
                        view.Text([Text.H2], "Review Before Submitting");
                        view.Text(["text-muted-foreground text-sm"],
                            "We found issues in your application. Please review and fix them before sending to Migri.");
                    });
                });

                // ── Score summary ─────────────────────────────────────────
                view.Row(["gap-4 flex-wrap"], content: view =>
                {
                    ReviewScorePill(view, "Completeness", result.CompletenessScore, isRisk: false);
                    ReviewScorePill(view, "Risk Score",   result.RiskScore,         isRisk: true);

                    // Predicted routing if submitted as-is
                    var (routeColor, routeLabel) = result.Routing switch
                    {
                        PermitReady.RoutingType.FastTrack           => ("text-success-primary bg-success-primary/10 border-success", "Will be fast-tracked ✓"),
                        PermitReady.RoutingType.SupplementRequested => ("text-warning-primary bg-warning-primary/10 border-warning", "Supplement request likely ⚠"),
                        _                                           => ("text-error-primary bg-error-primary/10 border-error",       "Manual review required ✗"),
                    };

                    view.Row([$"{routeColor} border rounded-lg px-4 py-3 flex-1 min-w-[180px] items-center gap-2"], content: view =>
                    {
                        view.Column(["gap-0"], content: view =>
                        {
                            view.Text(["text-xs text-muted-foreground"], "If submitted as-is:");
                            view.Text(["text-sm font-semibold " + routeColor.Split(' ')[0]], routeLabel);
                        });
                    });
                });

                // ── Missing items ─────────────────────────────────────────
                if (hasMissing)
                {
                    view.Column([Card.Default, "p-5 gap-3 border-l-4 border-l-warning"], content: view =>
                    {
                        view.Row(["items-center gap-2"], content: view =>
                        {
                            view.Icon(["text-warning-primary w-5 h-5 shrink-0"], name: "clipboard-list");
                            view.Text(["font-semibold text-base"], $"Missing Items ({result.MissingItems.Count})");
                        });

                        view.Text(["text-sm text-muted-foreground"],
                            "These items are required for a complete application. Go back and add them.");

                        view.Column(["gap-2 mt-1"], content: view =>
                        {
                            foreach (var item in result.MissingItems)
                            {
                                view.Row([Card.Default, "px-3 py-2.5 items-start gap-3"], content: view =>
                                {
                                    view.Box(["w-5 h-5 rounded border-2 border-warning shrink-0 mt-0.5 flex items-center justify-center"], content: view =>
                                        view.Icon(["text-warning-primary w-3 h-3"], name: "x"));

                                    view.Column(["gap-0.5 flex-1"], content: view =>
                                    {
                                        view.Text(["text-sm font-medium"], item);
                                        view.Text(["text-xs text-muted-foreground"], FixHintFor(item, input.PermitType));
                                    });
                                });
                            }
                        });
                    });
                }

                // ── Risk flags ────────────────────────────────────────────
                if (hasRiskFlags)
                {
                    view.Column([Card.Default, "p-5 gap-3 border-l-4 border-l-error"], content: view =>
                    {
                        view.Row(["items-center gap-2"], content: view =>
                        {
                            view.Icon(["text-error-primary w-5 h-5 shrink-0"], name: "shield-alert");
                            view.Text(["font-semibold text-base"], $"Flags Requiring Attention ({result.RiskFlags.Count})");
                        });

                        view.Text(["text-sm text-muted-foreground"],
                            "These issues may trigger additional scrutiny or delay your application. We recommend resolving them.");

                        view.Column(["gap-2 mt-1"], content: view =>
                        {
                            foreach (var flag in result.RiskFlags)
                            {
                                view.Row([Card.Default, "px-3 py-2.5 items-start gap-3"], content: view =>
                                {
                                    view.Box(["w-5 h-5 rounded bg-error-primary/10 border border-error shrink-0 mt-0.5 flex items-center justify-center"], content: view =>
                                        view.Icon(["text-error-primary w-3 h-3"], name: "alert-triangle"));

                                    view.Column(["gap-0.5 flex-1"], content: view =>
                                    {
                                        view.Text(["text-sm font-medium"], flag);
                                        view.Text(["text-xs text-muted-foreground"], RiskHintFor(flag));
                                    });
                                });
                            }
                        });
                    });
                }

                // ── What will happen if submitted as-is ───────────────────
                if (result.Routing != PermitReady.RoutingType.FastTrack)
                {
                    view.Row([Alert.Warning, "px-4 py-3 rounded-lg border items-start gap-3"], content: view =>
                    {
                        view.Icon(["text-warning-primary w-5 h-5 shrink-0 mt-0.5"], name: "info");
                        view.Column(["gap-1"], content: view =>
                        {
                            view.Text(["font-medium text-sm text-warning-primary"], "Submitting with issues will slow your application");
                            view.Text(["text-sm"], result.Routing == PermitReady.RoutingType.SupplementRequested
                                ? "Migri will send a supplement request email. You'll need to respond within 30 days, adding weeks to processing time."
                                : "Your application will be referred to a specialist officer for manual review, which can take significantly longer.");
                        });
                    });
                }

                // ── Action buttons ────────────────────────────────────────
                view.Column(["gap-3"], content: view =>
                {
                    // Primary action: go back and fix
                    view.Button([Button.PrimaryMd, "w-full"], "← Go Back and Fix Issues",
                        onClick: async () => Navigate("form"));

                    // Divider
                    view.Row(["items-center gap-3"], content: view =>
                    {
                        view.Box(["flex-1 h-px bg-border"]);
                        view.Text(["text-xs text-muted-foreground shrink-0"], "or");
                        view.Box(["flex-1 h-px bg-border"]);
                    });

                    // Secondary: proceed to payment despite issues
                    view.Column([Card.Default, "p-4 gap-3"], content: view =>
                    {
                        view.Text(["text-sm text-muted-foreground text-center"],
                            "If you believe your information is correct and complete, you can proceed to payment. Migri may contact you for clarification.");

                        view.Button([Button.SecondaryMd, "w-full"], "Proceed to Payment →",
                            onClick: async () => ConfirmSubmissionAsync());
                    });
                });
            });
        });
    }

    // ── Review step indicator ─────────────────────────────────────────────

    private static void RenderReviewStepper(UIView view)
    {
        var steps = new[] { "Fill Application", "Review", "Payment", "Submitted" };
        const int current = 1; // Review is step index 1

        view.Row(["items-start justify-center gap-0"], content: view =>
        {
            for (int i = 0; i < steps.Length; i++)
            {
                bool done   = i < current;
                bool active = i == current;

                view.Column(["items-center gap-1 w-24"], content: view =>
                {
                    view.Box([
                        done   ? "w-8 h-8 rounded-full bg-success-primary flex items-center justify-center" :
                        active ? "w-8 h-8 rounded-full bg-warning-primary flex items-center justify-center ring-4 ring-warning/20" :
                                 "w-8 h-8 rounded-full bg-muted border-2 border-border flex items-center justify-center"
                    ], content: v =>
                    {
                        if (done)
                            v.Icon(["w-4 h-4 text-white"], name: "check");
                        else
                            v.Text([active ? "text-xs font-bold text-white" : "text-xs text-muted-foreground"],
                                (i + 1).ToString());
                    });
                    view.Text([
                        active ? "text-[11px] font-semibold text-warning-primary text-center" :
                        done   ? "text-[11px] text-success-primary text-center" :
                                 "text-[11px] text-muted-foreground text-center"
                    ], steps[i]);
                });

                if (i < steps.Length - 1)
                    view.Box([done ? "w-8 h-0.5 bg-success-primary mt-4 shrink-0"
                                   : "w-8 h-0.5 bg-border mt-4 shrink-0"]);
            }
        });
    }

    // ── Contextual fix hints ──────────────────────────────────────────────

    private static string FixHintFor(string item, PermitReady.PermitType permitType)
    {
        var lower = item.ToLowerInvariant();
        if (lower.Contains("acceptance")) return "Upload your university acceptance letter (PDF). This is mandatory for student permits.";
        if (lower.Contains("transcript")) return "Upload official academic transcripts from your previous institution.";
        if (lower.Contains("passport copy")) return "Upload a scanned copy of your passport's photo page.";
        if (lower.Contains("passport expiry") || lower.Contains("passport validity")) return "Enter your passport expiry date. It must cover your entire permit period plus 6 months.";
        if (lower.Contains("funds") || lower.Contains("6,720")) return "Enter your monthly financial means. Students need at least €6,720/year (€560/month). Include bank statements.";
        if (lower.Contains("employment contract") || lower.Contains("contract")) return "Upload your signed employment contract from your Finnish employer.";
        if (lower.Contains("salary proof")) return "Upload recent payslips or a payroll confirmation letter from your employer.";
        if (lower.Contains("salary") || lower.Contains("1,500")) return "Enter your agreed monthly salary. It must be at or above the collective bargaining agreement (min. €1,500/month).";
        if (lower.Contains("university")) return "Enter the full official name of your Finnish educational institution.";
        if (lower.Contains("program")) return "Enter your degree program name (e.g. 'Master of Science in Computer Science').";
        if (lower.Contains("employer")) return "Enter the legal name of your Finnish employer as it appears on your contract.";
        if (lower.Contains("job title")) return "Enter your job title exactly as stated in your employment contract.";
        return "Please provide this information in the application form.";
    }

    private static string RiskHintFor(string flag)
    {
        var lower = flag.ToLowerInvariant();
        if (lower.Contains("suspicious document")) return "Rename your file to something descriptive (e.g. 'passport_copy.pdf', 'employment_contract.pdf').";
        if (lower.Contains("passport copy not provided")) return "Upload a clear scan of your passport. This is required for identity verification.";
        if (lower.Contains("expires very soon") || lower.Contains("expir")) return "Renew your passport before applying. It must be valid for your full permit period plus at least 6 months.";
        if (lower.Contains("funds critically")) return "Provide documentation showing sufficient financial means (bank statements, scholarship letter, etc.).";
        if (lower.Contains("salary critically")) return "Confirm the salary in your contract. If it's below the collective agreement minimum, your employer may need to revise the offer.";
        if (lower.Contains("generic employer")) return "Use the full legal name of your employer (e.g. 'Nokia Oyj' not just 'Nokia' or 'Company').";
        if (lower.Contains("suspicious") && lower.Contains("name")) return "Ensure your full legal name matches your passport exactly.";
        return "Review this information and ensure it matches your official documents.";
    }

    private static void ReviewScorePill(UIView view, string label, int score, bool isRisk)
    {
        string color = isRisk
            ? (score <= 25 ? "text-success-primary" : score <= 60 ? "text-warning-primary" : "text-error-primary")
            : (score >= 90 ? "text-success-primary" : score >= 70 ? "text-warning-primary" : "text-error-primary");

        view.Column([Card.Default, "px-4 py-3 flex-1 min-w-[120px] items-center gap-0.5"], content: view =>
        {
            view.Text(["text-2xl font-bold font-heading " + color], $"{score}");
            view.Text(["text-xs text-muted-foreground"], label);
        });
    }
}
