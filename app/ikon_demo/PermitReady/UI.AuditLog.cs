public partial class IkonDemoApp
{
    private void RenderAuditLogDialog(UIView view)
    {
        var appId   = _selectedAppId.Value ?? "";
        var entries = _chatAuditLog.Value
            .Where(e => e.ApplicationId == appId)
            .OrderByDescending(e => e.Timestamp)
            .ToList();

        view.Dialog(
            open: _showAuditLog.Value,
            onOpenChange: async o => { _showAuditLog.Value = o ?? false; },
            overlayStyle: [Dialog.Overlay],
            contentStyle: [Dialog.Content, "max-w-2xl w-full gap-0 p-0 overflow-hidden"],
            trigger: v => v.Box([]),
            content: view =>
            {
                // Header
                view.Row(["px-6 py-4 border-b border-border items-center gap-3"], content: view =>
                {
                    view.Box(["w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"],
                        content: v => v.Icon(["text-primary w-4 h-4"], name: "history"));
                    view.Column(["gap-0 flex-1"], content: view =>
                    {
                        view.Text(["font-semibold text-base"], "Chat Audit Log");
                        view.Text(["text-xs text-muted-foreground"],
                            $"Application {appId} · {entries.Count} recorded interaction{(entries.Count != 1 ? "s" : "")}");
                    });
                    view.Button([Button.GhostSm, Button.Size.Icon],
                        content: v => v.Icon(["w-4 h-4"], name: "x"),
                        onClick: async () => { _showAuditLog.Value = false; });
                });

                // Content
                view.ScrollArea(rootStyle: ["max-h-[70vh]"], content: view =>
                {
                    if (entries.Count == 0)
                    {
                        view.Column(["items-center py-16 gap-3"], content: view =>
                        {
                            view.Icon(["text-muted-foreground w-10 h-10"], name: "history");
                            view.Text(["text-muted-foreground font-medium"], "No interactions yet");
                            view.Text(["text-xs text-muted-foreground"], "Chat queries will appear here after officers interact with this application.");
                        });
                        return;
                    }

                    view.Column(["px-6 py-4 gap-4"], content: view =>
                    {
                        foreach (var entry in entries)
                        {
                            view.Column([Card.Default, "p-4 gap-3"], content: view =>
                            {
                                // Timestamp header
                                view.Row(["items-center gap-2 pb-2 border-b border-border/50"], content: view =>
                                {
                                    view.Icon(["text-muted-foreground w-3 h-3"], name: "clock");
                                    view.Text(["text-xs text-muted-foreground"],
                                        entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
                                });

                                // Officer query
                                view.Row(["gap-2 items-start"], content: view =>
                                {
                                    view.Box(["w-6 h-6 rounded-full bg-muted flex items-center justify-center shrink-0 mt-0.5"],
                                        content: v => v.Icon(["text-muted-foreground w-3 h-3"], name: "user"));
                                    view.Column(["gap-1 flex-1"], content: view =>
                                    {
                                        view.Text(["text-[10px] font-semibold text-muted-foreground uppercase tracking-wide"],
                                            "Officer Query");
                                        view.Box(["bg-muted/60 rounded-lg px-3 py-2"],
                                            content: v => v.Text(["text-xs"], entry.UserQuery));
                                    });
                                });

                                // AI response
                                view.Row(["gap-2 items-start"], content: view =>
                                {
                                    view.Box(["w-6 h-6 rounded-full bg-primary flex items-center justify-center shrink-0 mt-0.5"],
                                        content: v => v.Icon(["text-primary-foreground w-3 h-3"], name: "bot"));
                                    view.Column(["gap-1 flex-1"], content: view =>
                                    {
                                        view.Text(["text-[10px] font-semibold text-muted-foreground uppercase tracking-wide"],
                                            "AI Response");
                                        view.Box(["bg-background rounded-lg border border-border px-3 py-2"],
                                            content: v => v.Markdown(["text-xs prose prose-sm max-w-none dark:prose-invert leading-relaxed"],
                                                content: entry.AiResponse.Length > 800
                                                    ? entry.AiResponse[..800] + "…"
                                                    : entry.AiResponse));
                                    });
                                });
                            });
                        }
                    });
                });

                // Footer
                view.Row(["px-6 py-3 border-t border-border justify-between items-center"], content: view =>
                {
                    view.Text(["text-xs text-muted-foreground"],
                        $"Showing {entries.Count} of {_chatAuditLog.Value.Count} total interactions");
                    view.Button([Button.GhostSm], label: "Close",
                        onClick: async () => { _showAuditLog.Value = false; });
                });
            });
    }
}
