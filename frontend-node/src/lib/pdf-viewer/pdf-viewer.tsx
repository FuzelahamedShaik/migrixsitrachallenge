import { memo, useCallback, useEffect, useRef, useState } from 'react';
import * as pdfjsLib from 'pdfjs-dist';
import type { PDFDocumentProxy, PDFPageProxy, RenderTask } from 'pdfjs-dist';
import type { IkonUiComponentResolver, UiComponentRendererProps } from '@ikonai/sdk-react-ui';
import { useUiNode } from '@ikonai/sdk-react-ui';

// Point PDF.js worker at the bundled file
pdfjsLib.GlobalWorkerOptions.workerSrc = new URL(
  'pdfjs-dist/build/pdf.worker.mjs',
  import.meta.url,
).toString();

// ── Types ────────────────────────────────────────────────────────────────────

interface PdfViewerProps {
  url: string;
  filename?: string;
  onCloseId?: string;
  context: UiComponentRendererProps['context'];
}

// ── Inner PDF canvas renderer ─────────────────────────────────────────────────

function PdfCanvas({ page, scale }: { page: PDFPageProxy; scale: number }) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const renderTaskRef = useRef<RenderTask | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    // Cancel any in-flight render
    renderTaskRef.current?.cancel();

    const viewport = page.getViewport({ scale });
    canvas.width  = viewport.width;
    canvas.height = viewport.height;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const renderTask = page.render({ canvasContext: ctx, viewport });
    renderTaskRef.current = renderTask;

    renderTask.promise.catch((err) => {
      if (err?.name !== 'RenderingCancelledException') console.error('PDF render error:', err);
    });

    return () => { renderTaskRef.current?.cancel(); };
  }, [page, scale]);

  return <canvas ref={canvasRef} style={{ display: 'block', maxWidth: '100%' }} />;
}

// ── Main PDF viewer component ─────────────────────────────────────────────────

function PdfViewer({ url, filename, onCloseId, context }: PdfViewerProps) {
  const [pdf, setPdf]       = useState<PDFDocumentProxy | null>(null);
  const [page, setPage]     = useState<PDFPageProxy | null>(null);
  const [pageNum, setPageNum] = useState(1);
  const [scale, setScale]   = useState(1.4);
  const [loading, setLoading] = useState(true);
  const [error, setError]   = useState<string | null>(null);

  // Load PDF document
  useEffect(() => {
    if (!url) return;
    setLoading(true);
    setError(null);
    setPdf(null);
    setPage(null);
    setPageNum(1);

    const loadingTask = pdfjsLib.getDocument({ url, withCredentials: false });
    loadingTask.promise
      .then((doc) => { setPdf(doc); setLoading(false); })
      .catch((err) => { setError(`Failed to load PDF: ${err.message}`); setLoading(false); });

    return () => { loadingTask.destroy(); };
  }, [url]);

  // Load specific page
  useEffect(() => {
    if (!pdf) return;
    pdf.getPage(pageNum).then(setPage).catch(console.error);
  }, [pdf, pageNum]);

  const close = useCallback(() => {
    if (onCloseId) context.dispatchAction(onCloseId, {});
  }, [onCloseId, context]);

  const totalPages = pdf?.numPages ?? 0;

  return (
    <div style={{
      position: 'fixed', inset: 0, zIndex: 9999,
      background: 'rgba(0,0,0,0.75)', backdropFilter: 'blur(4px)',
      display: 'flex', flexDirection: 'column', alignItems: 'center',
    }}>
      {/* ── Toolbar ── */}
      <div style={{
        width: '100%', maxWidth: 900,
        display: 'flex', alignItems: 'center', gap: 12,
        padding: '10px 16px',
        background: 'hsl(var(--background))',
        borderBottom: '1px solid hsl(var(--border))',
        flexShrink: 0,
      }}>
        {/* File icon + name */}
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
          style={{ color: 'hsl(var(--primary))', flexShrink: 0 }}>
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
          <polyline points="14 2 14 8 20 8"/>
        </svg>
        <span style={{ fontSize: 13, fontWeight: 600, flex: 1, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
          {filename ?? 'Document'}
        </span>

        {/* Page controls */}
        {totalPages > 0 && (
          <div style={{ display: 'flex', alignItems: 'center', gap: 6, flexShrink: 0 }}>
            <button
              onClick={() => setPageNum(p => Math.max(1, p - 1))}
              disabled={pageNum <= 1}
              style={navBtnStyle(pageNum <= 1)}
              title="Previous page"
            >‹</button>
            <span style={{ fontSize: 12, color: 'hsl(var(--muted-foreground))' }}>
              {pageNum} / {totalPages}
            </span>
            <button
              onClick={() => setPageNum(p => Math.min(totalPages, p + 1))}
              disabled={pageNum >= totalPages}
              style={navBtnStyle(pageNum >= totalPages)}
              title="Next page"
            >›</button>
          </div>
        )}

        {/* Zoom controls */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 4, flexShrink: 0 }}>
          <button onClick={() => setScale(s => Math.max(0.5, +(s - 0.2).toFixed(1)))} style={iconBtnStyle} title="Zoom out">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/><line x1="8" y1="11" x2="14" y2="11"/>
            </svg>
          </button>
          <span style={{ fontSize: 11, color: 'hsl(var(--muted-foreground))', minWidth: 36, textAlign: 'center' }}>
            {Math.round(scale * 100)}%
          </span>
          <button onClick={() => setScale(s => Math.min(3, +(s + 0.2).toFixed(1)))} style={iconBtnStyle} title="Zoom in">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
              <line x1="11" y1="8" x2="11" y2="14"/><line x1="8" y1="11" x2="14" y2="11"/>
            </svg>
          </button>
          <button onClick={() => setScale(1.4)} style={iconBtnStyle} title="Reset zoom">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"/>
              <path d="M3 3v5h5"/>
            </svg>
          </button>
        </div>

        {/* Open in tab */}
        <a href={url} target="_blank" rel="noreferrer"
          style={{ ...iconBtnStyle, textDecoration: 'none', display: 'flex', alignItems: 'center' }}
          title="Open in new tab">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/>
            <polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/>
          </svg>
        </a>

        {/* Close */}
        <button onClick={close} style={{ ...iconBtnStyle, marginLeft: 4 }} title="Close (Esc)">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
          </svg>
        </button>
      </div>

      {/* ── PDF canvas area ── */}
      <div
        style={{ flex: 1, overflowY: 'auto', overflowX: 'auto', width: '100%', padding: '24px 0', display: 'flex', justifyContent: 'center' }}
        onClick={(e) => { if (e.target === e.currentTarget) close(); }}
      >
        {loading && (
          <div style={{ color: '#fff', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12, paddingTop: 80 }}>
            <div style={{ width: 32, height: 32, border: '3px solid rgba(255,255,255,0.3)', borderTopColor: '#fff', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
            <span style={{ fontSize: 14, opacity: 0.8 }}>Loading document…</span>
          </div>
        )}

        {error && (
          <div style={{ color: '#fca5a5', background: 'rgba(0,0,0,0.5)', borderRadius: 8, padding: '20px 32px', maxWidth: 400, textAlign: 'center' }}>
            <div style={{ fontSize: 32, marginBottom: 8 }}>⚠</div>
            <div style={{ fontSize: 14 }}>{error}</div>
          </div>
        )}

        {page && !loading && !error && (
          <div style={{ boxShadow: '0 8px 40px rgba(0,0,0,0.6)', background: '#fff', borderRadius: 2 }}>
            <PdfCanvas page={page} scale={scale} />
          </div>
        )}
      </div>

      {/* Keyboard listener */}
      <KeyboardHandler
        onEscape={close}
        onLeft={() => setPageNum(p => Math.max(1, p - 1))}
        onRight={() => setPageNum(p => Math.min(totalPages, p + 1))}
      />

      <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
    </div>
  );
}

// ── Keyboard shortcuts ────────────────────────────────────────────────────────

function KeyboardHandler({ onEscape, onLeft, onRight }: { onEscape: () => void; onLeft: () => void; onRight: () => void }) {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onEscape();
      if (e.key === 'ArrowLeft') onLeft();
      if (e.key === 'ArrowRight') onRight();
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [onEscape, onLeft, onRight]);
  return null;
}

// ── Styles ────────────────────────────────────────────────────────────────────

const iconBtnStyle: React.CSSProperties = {
  background: 'none', border: 'none', cursor: 'pointer',
  padding: '5px 6px', borderRadius: 6,
  color: 'hsl(var(--muted-foreground))',
  display: 'flex', alignItems: 'center', justifyContent: 'center',
  transition: 'background 0.15s',
};

const navBtnStyle = (disabled: boolean): React.CSSProperties => ({
  background: 'none', border: '1px solid hsl(var(--border))', borderRadius: 6,
  padding: '2px 10px', cursor: disabled ? 'default' : 'pointer',
  fontSize: 18, lineHeight: 1, color: disabled ? 'hsl(var(--muted-foreground))' : 'hsl(var(--foreground))',
  opacity: disabled ? 0.4 : 1,
});

// ── Ikon component wrapper ────────────────────────────────────────────────────

const PdfViewerRenderer = memo(function PdfViewerRenderer({ nodeId, context }: UiComponentRendererProps) {
  const node = useUiNode(context.store, nodeId);
  if (!node) return null;

  const url        = node.props?.['url']       as string | undefined;
  const filename   = node.props?.['filename']  as string | undefined;
  const onCloseId  = node.props?.['onCloseId'] as string | undefined;

  if (!url) return null;

  return <PdfViewer url={url} filename={filename} onCloseId={onCloseId} context={context} />;
});

export function createPdfViewerResolver(): IkonUiComponentResolver {
  return (node) => {
    if (node.type !== 'pdf-viewer') return undefined;
    return PdfViewerRenderer;
  };
}
