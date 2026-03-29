// Shared UI helpers used across multiple pages (Results + Detail)
public partial class IkonDemoApp
{
    // ── Supplement email generation ────────────────────────────────────────
    // Triggered when risk score > 40 (moderate or high risk)
    private static string GenerateSupplementEmail(
        string appId,
        PermitReady.ApplicationInput input,
        PermitReady.ScreeningResult result)
    {
        var permitLabel = input.Category.DisplayName();
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"To: {input.Email}");
        sb.AppendLine($"Subject: Supplement Request – Residence Permit Application {appId}");
        sb.AppendLine();
        sb.AppendLine($"Dear {input.FullName},");
        sb.AppendLine();
        sb.AppendLine($"Thank you for submitting your {permitLabel} permit application (Reference: {appId}).");
        sb.AppendLine();
        sb.AppendLine("After reviewing your application, we require additional information or documents before we can continue processing. Please provide the following within 30 days of this notice.");
        sb.AppendLine();

        if (result.MissingItems.Count > 0)
        {
            sb.AppendLine("MISSING DOCUMENTS / INFORMATION");
            sb.AppendLine(new string('-', 40));
            foreach (var item in result.MissingItems)
                sb.AppendLine($"  • {item}");
            sb.AppendLine();
        }

        if (result.RiskFlags.Count > 0)
        {
            sb.AppendLine("ITEMS REQUIRING CLARIFICATION");
            sb.AppendLine(new string('-', 40));
            foreach (var flag in result.RiskFlags)
                sb.AppendLine($"  • {flag}");
            sb.AppendLine();
        }

        sb.AppendLine("HOW TO RESPOND");
        sb.AppendLine(new string('-', 40));
        sb.AppendLine("  1. Log in to EnterFinland at enterfinland.fi");
        sb.AppendLine($"  2. Open application {appId}");
        sb.AppendLine("  3. Upload the requested documents or add a written explanation");
        sb.AppendLine("  4. Submit your response — we will continue processing once received");
        sb.AppendLine();
        sb.AppendLine("If you do not respond within 30 days, your application may be rejected.");
        sb.AppendLine();
        sb.AppendLine("CONTACT");
        sb.AppendLine(new string('-', 40));
        sb.AppendLine("  Phone:  +358 295 430 431 (Mon–Fri 8:00–16:00)");
        sb.AppendLine("  Email:  info@migri.fi");
        sb.AppendLine("  Portal: enterfinland.fi");
        sb.AppendLine();
        sb.AppendLine("Kind regards,");
        sb.AppendLine("Finnish Immigration Service (Migri)");
        sb.AppendLine("P.O. Box 10, FI-00086 Maahanmuuttovirasto");
        sb.AppendLine("Helsinki, Finland");

        return sb.ToString();
    }

    private static void RenderSupplementEmailBlock(UIView view, string appId,
        PermitReady.ApplicationInput input, PermitReady.ScreeningResult result, bool isOfficer)
    {
        var email = GenerateSupplementEmail(appId, input, result);

        view.Column([Card.Default, "p-6 gap-3"], content: view =>
        {
            view.Row(["items-center gap-3"], content: view =>
            {
                view.Box(["w-8 h-8 rounded-md bg-warning-primary/20 flex items-center justify-center shrink-0"], content: view =>
                    view.Icon(["text-warning-primary w-4 h-4"], name: "mail"));

                view.Column(["gap-0"], content: view =>
                {
                    view.Text(["font-semibold"],
                        isOfficer ? "Drafted Supplement Request Email" : "Supplement Request — What Migri Will Send You");
                    view.Text(["text-xs text-muted-foreground"],
                        isOfficer
                            ? $"Risk score {result.RiskScore}/100 exceeds threshold (40). Review and send to applicant."
                            : "A supplement request has been generated. Migri will send this to your email address.");
                });
            });

            // Email body in a monospace box
            view.Box(["bg-muted rounded-md p-4 border border-border"], content: view =>
                view.Text(["text-xs font-mono leading-relaxed whitespace-pre-wrap break-all"], email));

            if (isOfficer)
            {
                view.Row(["gap-2 flex-wrap"], content: view =>
                {
                    view.Box([Badge.Secondary, "text-xs"], content: v =>
                        v.Text([], $"To: {input.Email}"));
                    view.Box([Badge.Secondary, "text-xs"], content: v =>
                        v.Text([], $"Risk: {result.RiskScore}/100"));
                    view.Box([Badge.Secondary, "text-xs"], content: v =>
                        v.Text([], $"Missing: {result.MissingItems.Count} items"));
                });
            }
        });
    }
    private static void RenderRoutingBanner(UIView view, PermitReady.RoutingType routing)
    {
        var (alertStyle, icon, title, subtitle) = routing switch
        {
            PermitReady.RoutingType.FastTrack => (
                Alert.Success, "check-circle",
                "Fast Track — Automated Processing",
                "Completeness ≥ 90% and risk score ≤ 25"),
            PermitReady.RoutingType.SupplementRequested => (
                Alert.Warning, "alert-circle",
                "Supplement Requested",
                "Completeness ≥ 70% and risk score ≤ 60"),
            _ => (
                Alert.Danger, "x-circle",
                "Specialist Review Required",
                "Application requires manual officer review")
        };

        view.Row([alertStyle, "px-5 py-4 rounded-lg border items-center gap-4"], content: view =>
        {
            view.Icon(["w-7 h-7 shrink-0"], name: icon);
            view.Column(["gap-0.5"], content: view =>
            {
                view.Text(["font-semibold text-base"], title);
                view.Text(["text-sm opacity-80"], subtitle);
            });
        });
    }

    private static void ScoreCard(UIView view, string label, int score, bool isRisk)
    {
        string color = isRisk
            ? (score <= 25 ? "text-success-primary" : score <= 60 ? "text-warning-primary" : "text-error-primary")
            : (score >= 90 ? "text-success-primary" : score >= 70 ? "text-warning-primary" : "text-error-primary");

        view.Column([Card.Default, "p-6 flex-1 min-w-[140px] items-center gap-2"], content: view =>
        {
            view.Text(["text-4xl font-bold font-heading " + color], $"{score}");
            view.Text(["text-sm text-muted-foreground text-center"], label);
        });
    }
}
