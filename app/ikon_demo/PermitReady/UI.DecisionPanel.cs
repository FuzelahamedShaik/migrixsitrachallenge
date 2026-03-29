public partial class IkonDemoApp
{
    private void RenderDecisionDialog(UIView view)
    {
        var app = _applications.Value.FirstOrDefault(a => a.ApplicationId == _selectedAppId.Value);

        view.Dialog(
            open: _showDecisionPanel.Value,
            onOpenChange: async o =>
            {
                _showDecisionPanel.Value = o ?? false;
                if (!(_showDecisionPanel.Value)) { _pendingDecision.Value = ""; _decisionNote.Value = ""; }
            },
            overlayStyle: [Dialog.Overlay],
            contentStyle: [Dialog.Content, "max-w-lg w-full gap-0 p-0 overflow-hidden"],
            trigger: v => v.Box([]),
            content: view =>
            {
                if (app == null) return;

                // Header
                view.Row(["px-6 py-4 border-b border-border items-center gap-3"], content: view =>
                {
                    view.Box(["w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"],
                        content: v => v.Icon(["text-primary w-4 h-4"], name: "gavel"));
                    view.Column(["gap-0"], content: view =>
                    {
                        view.Text(["font-semibold text-base"], "Officer Decision");
                        view.Text(["text-xs text-muted-foreground"],
                            $"Application {app.ApplicationId} · {app.Input.FullName}");
                    });
                });

                // Body
                view.Column(["px-6 py-4 gap-4"], content: view =>
                {
                    view.Text(["text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-1"],
                        "Select your decision:");

                    // Approve
                    RenderDecisionCard(view, "approve", "check-circle",
                        "Approve — Proceed to Biometrics",
                        "Application meets all requirements. Move applicant to biometrics appointment stage.",
                        "border-success-primary/30 hover:bg-success-primary/5",
                        "bg-success-primary/10 border-success-primary",
                        "text-success-primary");

                    // Supplement
                    RenderDecisionCard(view, "supplement", "alert-circle",
                        "Request Supplement",
                        "Additional documents or clarification required from the applicant.",
                        "border-warning-primary/30 hover:bg-warning-primary/5",
                        "bg-warning-primary/10 border-warning-primary",
                        "text-warning-primary");

                    // Reject
                    RenderDecisionCard(view, "reject", "x-circle",
                        "Reject Application",
                        "Application does not meet Finnish immigration requirements.",
                        "border-error-primary/30 hover:bg-error-primary/5",
                        "bg-error-primary/10 border-error-primary",
                        "text-error-primary");

                    // Notes textarea
                    if (!string.IsNullOrEmpty(_pendingDecision.Value))
                    {
                        view.Column(["gap-1.5 mt-1"], content: view =>
                        {
                            view.Text(["text-xs font-medium text-muted-foreground"], "Officer notes (optional):");
                            view.TextArea([Input.Default, "min-h-[70px] resize-none text-sm"],
                                placeholder: _pendingDecision.Value switch
                                {
                                    "approve"    => "e.g. All documents verified. Salary above threshold.",
                                    "supplement" => "e.g. Please provide employment contract and salary proof.",
                                    _            => "e.g. Salary below EU Blue Card minimum (€4,867). Recommend employee permit.",
                                },
                                value: _decisionNote.Value,
                                onValueChange: async v => { _decisionNote.Value = v; });
                        });
                    }
                });

                // Footer
                view.Row(["px-6 py-4 border-t border-border justify-end gap-2"], content: view =>
                {
                    view.Button([Button.GhostSm], label: "Cancel",
                        onClick: async () =>
                        {
                            _showDecisionPanel.Value = false;
                            _pendingDecision.Value   = "";
                            _decisionNote.Value      = "";
                        });

                    if (!string.IsNullOrEmpty(_pendingDecision.Value))
                    {
                        var btnStyle = _pendingDecision.Value switch
                        {
                            "approve" => Button.PrimaryMd,
                            _         => Button.OutlineMd,
                        };
                        var btnLabel = _pendingDecision.Value switch
                        {
                            "approve"    => "Approve & Proceed →",
                            "supplement" => "Send Supplement Request",
                            _            => "Submit Rejection",
                        };

                        view.Button([btnStyle],
                            label: btnLabel,
                            disabled: _decisionSubmitting.Value,
                            onClick: async () =>
                            {
                                _decisionSubmitting.Value = true;
                                SubmitOfficerDecision(app.ApplicationId, _pendingDecision.Value, _decisionNote.Value);
                            });
                    }
                });
            });
    }

    private void RenderDecisionCard(
        UIView view,
        string decision,
        string icon,
        string label,
        string desc,
        string defaultBorder,
        string selectedBg,
        string textColor)
    {
        bool sel = _pendingDecision.Value == decision;
        view.Button(
            [sel ? $"border rounded-lg px-4 py-3 w-full text-left items-start gap-3 h-auto {selectedBg} border-2" : $"border rounded-lg px-4 py-3 w-full text-left items-start gap-3 h-auto bg-background {defaultBorder} hover:bg-muted/30"],
            content: v =>
            {
                v.Icon([$"{textColor} w-4 h-4 shrink-0 mt-0.5"], name: icon);
                v.Column(["gap-0.5 flex-1 text-left"], content: v =>
                {
                    v.Text([$"text-sm font-medium {(sel ? textColor : "text-foreground")}"], label);
                    v.Text(["text-xs text-muted-foreground leading-snug"], desc);
                });
                if (sel)
                    v.Icon([$"{textColor} w-4 h-4 shrink-0 mt-0.5"], name: "check-circle");
            },
            onClick: async () => { _pendingDecision.Value = decision; });
    }
}
