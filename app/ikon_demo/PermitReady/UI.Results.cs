public partial class IkonDemoApp
{
    private void RenderResults(UIView view)
    {
        var result = _lastResult.Value;
        var input  = _lastInput.Value;

        if (result == null || input == null)
        {
            Navigate("form");
            return;
        }

        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column([Container.Md, "py-10 px-4 gap-6 min-h-full"], content: view =>
            {
                // Back button
                view.Button([Button.GhostSm, "w-fit -ml-2"], "← Back to Home",
                    onClick: async () => Navigate("applicant_home"));

                // Header + App ID
                view.Row(["items-start justify-between gap-4 flex-wrap"], content: view =>
                {
                    view.Column(["gap-1"], content: view =>
                    {
                        view.Text([Text.H2], "Application Checked");
                        view.Text(["text-muted-foreground"], $"Results for {input.FullName} · {input.Category.DisplayName()}");
                    });

                    // Reference number pill (applicant needs this for tracking)
                    view.Column([Card.Default, "px-4 py-3 items-center gap-0.5 shrink-0"], content: view =>
                    {
                        view.Text(["text-xs text-muted-foreground"], "Your Reference Number");
                        view.Text(["text-xl font-bold font-mono tracking-wider"], _lastAppId.Value);
                        view.Text(["text-xs text-muted-foreground"], "Save this to track your application");
                    });
                });

                // Routing verdict banner
                RenderRoutingBanner(view, result.Routing);

                // Score cards row
                view.Row(["gap-4 flex-wrap"], content: view =>
                {
                    ScoreCard(view, "Completeness", result.CompletenessScore, isRisk: false);
                    ScoreCard(view, "Risk Score",   result.RiskScore,         isRisk: true);
                });

                // Progress bars
                view.Column([Card.Default, "p-6 gap-4"], content: view =>
                {
                    view.Text([Text.H3], "Score Breakdown");

                    ProgressRow(view, "Completeness", result.CompletenessScore,
                        result.CompletenessScore >= 90 ? Progress.Variant.Success :
                        result.CompletenessScore >= 70 ? Progress.Variant.Warning :
                        Progress.Variant.Error);

                    ProgressRow(view, "Risk Level", result.RiskScore,
                        result.RiskScore <= 25 ? Progress.Variant.Success :
                        result.RiskScore <= 60 ? Progress.Variant.Warning :
                        Progress.Variant.Error);
                });

                // Missing items
                if (result.MissingItems.Count > 0)
                {
                    view.Column([Alert.Warning, "px-5 py-4 rounded-lg border gap-3"], content: view =>
                    {
                        view.Text(["font-semibold text-warning-primary"], $"Missing Items ({result.MissingItems.Count})");
                        view.Column(["gap-1"], content: view =>
                        {
                            foreach (var item in result.MissingItems)
                                view.Row(["gap-2 items-start"], content: view =>
                                {
                                    view.Text(["text-warning-primary mt-0.5"], "·");
                                    view.Text(["text-sm"], item);
                                });
                        });
                    });
                }

                // Risk flags (only shown if any)
                if (result.RiskFlags.Count > 0)
                {
                    view.Column([Alert.Danger, "px-5 py-4 rounded-lg border gap-3"], content: view =>
                    {
                        view.Text(["font-semibold text-error-primary"], $"Risk Flags ({result.RiskFlags.Count})");
                        view.Column(["gap-1"], content: view =>
                        {
                            foreach (var flag in result.RiskFlags)
                                view.Row(["gap-2 items-start"], content: view =>
                                {
                                    view.Text(["text-error-primary mt-0.5"], "⚠");
                                    view.Text(["text-sm"], flag);
                                });
                        });
                    });
                }

                // Supplement email draft (risk > 40)
                if (result.RiskScore > 40 && !string.IsNullOrEmpty(_lastAppId.Value))
                {
                    RenderSupplementEmailBlock(view, _lastAppId.Value, input, result, isOfficer: false);
                }

                // What happens next
                view.Column([Card.Default, "p-6 gap-3"], content: view =>
                {
                    view.Text([Text.H3], "What happens next?");
                    view.Text(["text-sm text-muted-foreground"], result.Routing switch
                    {
                        PermitReady.RoutingType.FastTrack =>
                            "Your application is complete and looks great! It has been fast-tracked for automated processing. You'll receive a decision by post and in EnterFinland within 6–8 weeks.",
                        PermitReady.RoutingType.SupplementRequested =>
                            "Your application is mostly complete. A supplement request email has been generated above — Migri will contact you with the details. Address the missing items within 30 days.",
                        _ =>
                            "Your application has been referred to a Migri specialist for manual review. A case officer will contact you within 5 business days. No action is required from you at this stage."
                    });
                });

                // Action buttons
                view.Row(["gap-3 flex-wrap"], content: view =>
                {
                    view.Button([Button.SecondaryMd], "← Back to Home",
                        onClick: async () => Navigate("applicant_home"));

                    view.Button([Button.GhostMd], "Submit Another Application",
                        onClick: async () =>
                        {
                            ResetForm();
                            Navigate("form");
                        });
                });
            });
        });
    }

    // ── Sub-helpers ────────────────────────────────────────────────────────

    private static void ProgressRow(UIView view, string label, int value, string variantStyle)
    {
        view.Column(["gap-1.5"], content: view =>
        {
            view.Row(["justify-between"], content: view =>
            {
                view.Text(["text-sm font-medium"], label);
                view.Text(["text-sm text-muted-foreground"], $"{value}/100");
            });
            view.Box([Progress.Root], content: view =>
                view.Box([Progress.Indicator, variantStyle, Progress.IndicatorTransform(value)]));
        });
    }

    private void ResetForm()
    {
        _fullName.Value       = "";
        _nationality.Value    = "";
        _email.Value          = "";
        _phone.Value          = "";
        _passportNumber.Value = "";
        _passportExpiry.Value = "";
        _universityName.Value = "";
        _programName.Value    = "";
        _fundsAmount.Value    = "";
        _studyStartDate.Value = "";
        _employerName.Value   = "";
        _jobTitle.Value       = "";
        _salaryAmount.Value   = "";
        _contractRef.Value    = "";
        _workStartDate.Value  = "";
        _passportDoc.Value      = null;
        _acceptanceDoc.Value    = null;
        _transcriptDoc.Value    = null;
        _bankStatementDoc.Value = null;
        _contractDoc.Value      = null;
        _salaryProofDoc.Value   = null;
        _formError.Value      = "";
        _lastResult.Value     = null;
        _lastInput.Value      = null;
    }
}
