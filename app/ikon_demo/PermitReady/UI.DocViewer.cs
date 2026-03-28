public partial class IkonDemoApp
{
    private void RenderDocViewerDialog(UIView view)
    {
        var app = _applications.Value.FirstOrDefault(a => a.ApplicationId == _viewingDocAppId.Value);
        var filename = _viewingDocFile.Value;

        PermitReady.DocPreview? preview = null;
        string pdfUrl = "";

        if (app != null && !string.IsNullOrEmpty(filename))
        {
            preview = PermitReady.DummyDocumentFactory.GetPreview(filename, app.Input);
            pdfUrl  = $"{_docsEndpointUrl}/pdf?appId={_viewingDocAppId.Value}&file={Uri.EscapeDataString(filename)}";
        }

        view.Dialog(
            open: _docViewerOpen.Value,
            onOpenChange: async o => { _docViewerOpen.Value = o ?? false; },
            overlayStyle: [Dialog.Overlay],
            contentStyle: [Dialog.Content, "max-w-2xl w-full p-0 overflow-hidden gap-0"],
            trigger: v => v.Box([]),
            content: view =>
            {
                if (preview == null)
                {
                    view.Box([Dialog.Header, "px-6 py-4"], content: v =>
                        v.Text([Dialog.Title], "Document not found"));
                    return;
                }

                // ── Header ────────────────────────────────────────────────
                view.Row(["px-6 py-4 border-b border-border items-center gap-3"], content: view =>
                {
                    view.Box(["w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"],
                        content: v => v.Icon(["text-primary w-4 h-4"], name: "file-text"));

                    view.Column(["flex-1 gap-0 min-w-0"], content: view =>
                    {
                        view.Text(["font-semibold text-sm truncate"], preview.DocType);
                        view.Text(["text-xs text-muted-foreground truncate"],
                            $"{preview.IssuerName} · {preview.IssuerCountry}");
                    });

                    view.Button([Button.GhostSm, Button.Size.Icon, "shrink-0"],
                        content: v => v.Icon(["w-4 h-4"], name: "x"),
                        onClick: async () => { _docViewerOpen.Value = false; });
                });

                // ── Meta chips ────────────────────────────────────────────
                view.Row(["px-6 py-3 gap-3 flex-wrap border-b border-border bg-muted/30"], content: view =>
                {
                    DocMetaChip(view, "calendar",   "Issued",     preview.DocDate);
                    DocMetaChip(view, "hash",        "Reference",  preview.DocNumber);
                    DocMetaChip(view, "map-pin",     "Country",    preview.IssuerCountry);
                });

                // ── Field table (scrollable) ───────────────────────────────
                view.ScrollArea(rootStyle: ["max-h-[52vh]"], content: view =>
                {
                    view.Column(["px-6 py-4 gap-4"], content: view =>
                    {
                        // Document header block (mimics official doc styling)
                        view.Column(["bg-muted/40 rounded-lg border border-border overflow-hidden"], content: view =>
                        {
                            // Doc type banner
                            view.Box(["bg-primary/5 border-b border-border px-4 py-2"], content: view =>
                            {
                                view.Text(["text-xs font-bold tracking-wider text-primary uppercase"],
                                    preview.DocType);
                            });

                            // Field rows
                            foreach (var field in preview.Fields)
                            {
                                view.Row(["px-4 py-2.5 border-b border-border/50 last:border-0 gap-4"],
                                    content: view =>
                                    {
                                        view.Text(["text-xs text-muted-foreground w-40 shrink-0"],
                                            field.Label);
                                        view.Text(["text-xs font-mono font-medium flex-1 break-all"],
                                            field.Value);
                                    });
                            }
                        });

                        // Footer note
                        view.Row(["bg-muted/50 rounded-lg px-4 py-3 items-start gap-2 border border-border/50"],
                            content: view =>
                            {
                                view.Icon(["text-muted-foreground w-3.5 h-3.5 shrink-0 mt-0.5"],
                                    name: "shield-check");
                                view.Text(["text-xs text-muted-foreground italic leading-relaxed"],
                                    preview.FooterNote);
                            });

                        // Demo disclaimer badge
                        view.Row(["items-center gap-1.5 justify-center py-1"], content: view =>
                        {
                            view.Icon(["text-warning-primary w-3 h-3"], name: "alert-triangle");
                            view.Text(["text-[10px] text-muted-foreground"],
                                "Demo document — not a real official document");
                        });
                    });
                });

                // ── Footer ────────────────────────────────────────────────
                view.Row(["px-6 py-4 border-t border-border items-center justify-between bg-background"],
                    content: view =>
                    {
                        view.Text(["text-xs text-muted-foreground font-mono truncate max-w-[60%]"], filename);

                        view.Row(["gap-2"], content: view =>
                        {
                            view.Button([Button.GhostSm],
                                label: "Close",
                                onClick: async () => { _docViewerOpen.Value = false; });

                            // Opens the generated PDF in a new browser tab
                            view.Text(
                                [Button.PrimarySm, "inline-flex items-center gap-1.5 no-underline cursor-pointer"],
                                "Open PDF ↗",
                                href: pdfUrl,
                                target: "_blank");
                        });
                    });
            });
    }

    private static void DocMetaChip(UIView view, string icon, string label, string value)
    {
        view.Row(["items-center gap-1.5 bg-background rounded-md border border-border/60 px-3 py-1.5"],
            content: view =>
            {
                view.Icon(["w-3 h-3 text-muted-foreground shrink-0"], name: icon);
                view.Column(["gap-0"], content: view =>
                {
                    view.Text(["text-[10px] text-muted-foreground uppercase tracking-wide leading-none"],
                        label);
                    view.Text(["text-xs font-medium leading-snug"], value);
                });
            });
    }
}
