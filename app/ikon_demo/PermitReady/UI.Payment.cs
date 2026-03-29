public partial class IkonDemoApp
{
    private void RenderPayment(UIView view)
    {
        var input  = _lastInput.Value;
        var result = _lastResult.Value;

        if (input == null || result == null) { Navigate("form"); return; }

        var fee        = GetApplicationFee(input.Category);
        var feeDisplay = fee.ToString("N0");
        var processing = _paymentProcessing.Value;

        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column([Container.Sm, "py-10 px-4 gap-6 min-h-full"], content: view =>
            {
                // ── Application journey stepper ───────────────────────────
                RenderPaymentStepper(view, input.Category);

                // ── Header ────────────────────────────────────────────────
                view.Row(["items-center gap-3"], content: view =>
                {
                    view.Box(["w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center shrink-0"],
                        content: v => v.Icon(["text-primary w-5 h-5"], name: "credit-card"));
                    view.Column(["gap-0.5"], content: view =>
                    {
                        view.Text([Text.H2], "Application Processing Fee");
                        view.Text(["text-muted-foreground text-sm"],
                            "Payment is required before Migri can begin processing your application.");
                    });
                });

                // ── Fee summary card ──────────────────────────────────────
                view.Column([Card.Default, "p-6 gap-4"], content: view =>
                {
                    view.Row(["items-start justify-between border-b border-border pb-4 gap-4"], content: view =>
                    {
                        view.Column(["gap-1 flex-1"], content: view =>
                        {
                            view.Text(["font-semibold text-sm"], $"{input.Category.DisplayName()}");
                            view.Text(["text-xs text-muted-foreground"], $"Applicant: {input.FullName}");
                            view.Text(["text-xs text-muted-foreground"], "Online submission · e-service (EnterFinland)");
                        });
                        view.Column(["items-end gap-0 shrink-0"], content: view =>
                        {
                            view.Text(["text-3xl font-bold font-heading text-primary"], $"€{feeDisplay}");
                            view.Text(["text-xs text-muted-foreground text-right"], "Online fee (incl. VAT)");
                        });
                    });

                    view.Column(["gap-2"], content: view =>
                    {
                        PaymentFeeRow(view, "Processing fee (online submission)",
                            $"€{feeDisplay}", bold: false);
                        PaymentFeeRow(view, "Online vs. paper discount",
                            fee < 750m ? $"−€{(750m - fee):N0} vs paper" : "−€50 vs paper", bold: false);
                        PaymentFeeRow(view, "Non-refundable — applies regardless of decision",
                            "Migri policy", bold: false);
                        view.Box(["h-px bg-border"]);
                        PaymentFeeRow(view, "Total due today", $"€{feeDisplay}", bold: true);
                    });
                });

                // ── Important notice ──────────────────────────────────────
                view.Row([Alert.Warning, "px-4 py-3 rounded-lg border items-start gap-3"], content: view =>
                {
                    view.Icon(["text-warning-primary w-5 h-5 shrink-0 mt-0.5"], name: "info");
                    view.Column(["gap-1"], content: view =>
                    {
                        view.Text(["font-medium text-sm text-warning-primary"],
                            "Processing begins only after payment is confirmed");
                        view.Text(["text-sm"],
                            "Your application will be queued for a Migri officer as soon as your payment clears. The fee is non-refundable.");
                    });
                });

                // ── Payment method selector ───────────────────────────────
                view.Column([Card.Default, "p-5 gap-4"], content: view =>
                {
                    view.Text(["font-semibold text-sm"], "Select Payment Method");

                    view.Column(["gap-2"], content: view =>
                    {
                        PaymentMethodOption(view, "online-banking", "landmark",
                            "Online Banking",
                            "Finnish banks — Nordea, OP, Danske Bank, Handelsbanken, S-Pankki",
                            processing);
                        PaymentMethodOption(view, "credit-card", "credit-card",
                            "Credit / Debit Card",
                            "Visa, Visa Electron, Mastercard",
                            processing);
                    });
                });

                // ── Security note ─────────────────────────────────────────
                view.Row(["items-center gap-2 justify-center"], content: view =>
                {
                    view.Icon(["w-4 h-4 text-muted-foreground"], name: "shield-check");
                    view.Text(["text-xs text-muted-foreground"],
                        "Secured by Nets — 256-bit SSL · PCI DSS Level 1 compliant");
                });

                // ── Action buttons ────────────────────────────────────────
                view.Column(["gap-3"], content: view =>
                {
                    if (processing)
                    {
                        view.Column([Card.Default, "p-5 items-center gap-3"], content: view =>
                        {
                            view.Row(["items-center gap-3"], content: view =>
                            {
                                view.Box(["w-5 h-5 rounded-full border-2 border-primary border-t-transparent animate-spin shrink-0"]);
                                view.Text(["font-medium text-sm"], "Connecting to payment gateway...");
                            });
                            view.Text(["text-xs text-muted-foreground text-center"],
                                "Please do not close this window. You will be redirected automatically.");
                        });
                    }
                    else
                    {
                        view.Button([Button.PrimaryMd, "w-full"],
                            $"Pay €{feeDisplay} Securely →",
                            onClick: async () => await ProcessPaymentAsync());

                        view.Button([Button.GhostMd, "w-full"], "← Back to Review",
                            onClick: async () => Navigate("review"));
                    }
                });
            });
        });
    }

    // ── Journey stepper (Fill → Review → Payment → Submitted) ────────────

    private static void RenderPaymentStepper(UIView view, PermitReady.PermitCategory cat)
    {
        var steps = new[] { "Fill Application", "Review", "Payment", "Submitted" };
        const int current = 2; // 0-indexed — Payment is active

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
                        active ? "w-8 h-8 rounded-full bg-primary flex items-center justify-center ring-4 ring-primary/20" :
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
                        active ? "text-[11px] font-semibold text-primary text-center" :
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

    // ── Payment method radio option ───────────────────────────────────────

    private void PaymentMethodOption(UIView view, string value, string icon,
        string label, string subtitle, bool disabled)
    {
        bool selected = _paymentMethod.Value == value;

        view.Button(
            [Card.Default,
             selected ? "border-primary bg-primary/5" : "hover:bg-muted/40",
             "w-full px-4 py-3 flex flex-row items-center gap-3 text-left transition-colors"],
            disabled: disabled,
            onClick: async () => { _paymentMethod.Value = value; },
            content: view =>
            {
                // Radio circle
                view.Box([
                    selected
                        ? "w-5 h-5 rounded-full border-2 border-primary bg-primary flex items-center justify-center shrink-0"
                        : "w-5 h-5 rounded-full border-2 border-border shrink-0"
                ], content: v =>
                {
                    if (selected)
                        v.Box(["w-2 h-2 rounded-full bg-white"]);
                });

                view.Icon(["w-5 h-5 text-muted-foreground shrink-0"], name: icon);

                view.Column(["gap-0 flex-1"], content: view =>
                {
                    view.Text(["text-sm font-medium"], label);
                    view.Text(["text-xs text-muted-foreground"], subtitle);
                });
            });
    }

    // ── Fee line item ─────────────────────────────────────────────────────

    private static void PaymentFeeRow(UIView view, string label, string value, bool bold)
    {
        view.Row(["justify-between items-baseline gap-4"], content: view =>
        {
            view.Text([bold ? "text-sm font-semibold" : "text-sm text-muted-foreground", "flex-1"], label);
            view.Text([bold ? "text-sm font-bold" : "text-sm text-muted-foreground"], value);
        });
    }
}
