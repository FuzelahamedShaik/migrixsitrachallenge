public partial class IkonDemoApp
{
    /// <summary>
    /// Renders the PDF viewer overlay when a document is selected.
    /// Uses a base64 data URL — no HTTP endpoint, no mixed-content issues.
    /// X / Esc / backdrop click closes it.
    /// </summary>
    private void RenderDocViewerDialog(UIView view)
    {
        if (!_docViewerOpen.Value) return;

        var dataUrl  = _viewingDocDataUrl.Value;
        var filename = _viewingDocFile.Value;

        // Loading state — data URL is being generated async
        if (string.IsNullOrEmpty(dataUrl))
        {
            view.Box(["fixed inset-0 z-50 bg-background/80 backdrop-blur-sm flex items-center justify-center"],
                content: v => v.Column(["items-center gap-3"], content: v =>
                {
                    v.Box(["w-8 h-8 rounded-full border-2 border-primary border-t-transparent animate-spin"]);
                    v.Text(["text-sm text-muted-foreground"], "Loading document…");
                }));
            return;
        }

        view.PdfViewer(
            url:      dataUrl,
            filename: filename,
            onClose:  async () =>
            {
                _docViewerOpen.Value     = false;
                _viewingDocAppId.Value   = "";
                _viewingDocFile.Value    = "";
                _viewingDocDataUrl.Value = "";
            });
    }
}
