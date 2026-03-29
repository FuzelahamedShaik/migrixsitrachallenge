using System.Runtime.CompilerServices;

// ── C# extension that wires the 'pdf-viewer' React component ─────────────────

public static class PdfViewerExtensions
{
    /// <summary>
    /// Renders an embedded full-screen PDF viewer (PDF.js) as an overlay.
    /// The viewer closes when the user presses Esc, clicks the backdrop, or clicks X.
    /// </summary>
    public static void PdfViewer(
        this UIView view,
        string url,
        string? filename = null,
        Func<Task>? onClose = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line  = 0)
    {
        string? onCloseId = null;

        if (onClose != null)
            onCloseId = view.CreateAction<object>(_ => onClose());

        view.AddNode(
            "pdf-viewer",
            new Dictionary<string, object?>
            {
                ["url"]       = url,
                ["filename"]  = filename,
                ["onCloseId"] = onCloseId,
            },
            file: file,
            line: line);
    }
}
