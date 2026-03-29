public partial class IkonDemoApp
{
    /// <summary>
    /// GDPR Article 5(1)(b) compliance — officer must state their reason before
    /// accessing an applicant's personal data. Logged for audit purposes.
    /// </summary>
    private void RenderAccessReasonDialog(UIView view)
    {
        if (!_showAccessReasonDialog.Value) return;

        var appId = _pendingAccessAppId.Value;
        var pendingApp = _applications.Value.FirstOrDefault(a => a.ApplicationId == appId);

        // Backdrop
        view.Box(["fixed inset-0 z-50 bg-background/80 backdrop-blur-sm flex items-center justify-center p-4"],
            content: view =>
            {
                view.Column([Card.Default, "w-full max-w-md gap-0 overflow-hidden shadow-2xl"], content: view =>
                {
                    // Header
                    view.Column(["px-6 pt-6 pb-4 border-b border-border gap-1"], content: view =>
                    {
                        view.Row(["items-center gap-2.5"], content: view =>
                        {
                            view.Box(["w-8 h-8 rounded-lg bg-warning-primary/15 flex items-center justify-center shrink-0"],
                                content: v => v.Icon(["text-warning-primary w-4 h-4"], name: "shield-alert"));
                            view.Column(["gap-0"], content: view =>
                            {
                                view.Text(["font-semibold text-sm"], "File Access Justification");
                                view.Text(["text-xs text-muted-foreground"], "GDPR Article 5(1)(b) — Purpose Limitation");
                            });
                        });
                    });

                    // Body
                    view.Column(["px-6 py-5 gap-4"], content: view =>
                    {
                        // Application context chip
                        if (pendingApp != null)
                        {
                            view.Row(["items-center gap-2 bg-muted/60 rounded-lg px-3 py-2.5"], content: view =>
                            {
                                view.Box(["w-7 h-7 rounded-full bg-primary/10 flex items-center justify-center shrink-0"],
                                    content: v => v.Icon(["text-primary w-3.5 h-3.5"], name: "user"));
                                view.Column(["gap-0 flex-1 min-w-0"], content: view =>
                                {
                                    view.Text(["text-xs font-semibold truncate"], pendingApp.Input.FullName);
                                    view.Text(["text-[10px] text-muted-foreground"],
                                        $"{appId} · {pendingApp.Input.Category.ShortName()}");
                                });
                            });
                        }

                        view.Text(["text-xs text-muted-foreground leading-relaxed"],
                            "Accessing this applicant's personal data must be purpose-limited. Your reason will be recorded in the audit log and may be reviewed by data protection officers.");

                        // Reason field
                        view.Column(["gap-1.5"], content: view =>
                        {
                            view.Text(["text-xs font-medium"], "Reason for Access *");
                            view.TextArea(
                                [Input.Default, "resize-none min-h-[90px] text-sm"],
                                placeholder: "e.g. Reviewing incomplete documents to determine supplement requirements for work permit application...",
                                value: _accessReason.Value,
                                onValueChange: async v =>
                                {
                                    _accessReason.Value      = v;
                                    _accessReasonError.Value = "";
                                });

                            if (!string.IsNullOrEmpty(_accessReasonError.Value))
                                view.Text(["text-xs text-error-primary"], _accessReasonError.Value);
                        });

                        // Audit notice
                        view.Row(["items-start gap-2 bg-muted/40 rounded-md px-3 py-2"], content: view =>
                        {
                            view.Icon(["text-muted-foreground w-3.5 h-3.5 mt-0.5 shrink-0"], name: "info");
                            view.Text(["text-[11px] text-muted-foreground leading-snug"],
                                "This access reason is logged with your timestamp and cannot be modified after confirmation.");
                        });
                    });

                    // Footer
                    view.Row(["px-6 pb-6 gap-3 justify-end"], content: view =>
                    {
                        view.Button([Button.OutlineMd],
                            "Cancel",
                            onClick: async () =>
                            {
                                _showAccessReasonDialog.Value = false;
                                _pendingAccessAppId.Value     = "";
                                _accessReason.Value           = "";
                                _accessReasonError.Value      = "";
                            });

                        view.Button([Button.PrimaryMd, "gap-2"],
                            content: v =>
                            {
                                v.Icon(["w-4 h-4"], name: "shield-check");
                                v.Text([], "Confirm & Open File");
                            },
                            disabled: string.IsNullOrWhiteSpace(_accessReason.Value),
                            onClick: async () => await ConfirmProfileAccessAndNavigateAsync());
                    });
                });
            });
    }
}
