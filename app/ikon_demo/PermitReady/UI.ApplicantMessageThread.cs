public partial class IkonDemoApp
{
    // ── Applicant-facing message thread ──────────────────────────────────────

    private void RenderApplicantMessageThread(UIView view, PermitReady.StoredApplication app)
    {
        // Only show officer messages and system notifications (not profile-access notices)
        var officerMsgs = _officialMessages.Value
            .Where(m => m.ApplicationId == app.ApplicationId &&
                        m.Type != PermitReady.OfficialMessageType.ProfileAccessNotice &&
                        m.Type != PermitReady.OfficialMessageType.ApplicantReply)
            .OrderBy(m => m.SentAt)
            .ToList();

        // All messages (for full conversation thread view)
        var allVisible = _officialMessages.Value
            .Where(m => m.ApplicationId == app.ApplicationId &&
                        m.Type != PermitReady.OfficialMessageType.ProfileAccessNotice)
            .OrderBy(m => m.SentAt)
            .ToList();

        var unread = UnreadOfficerMessages(app.ApplicationId);

        if (officerMsgs.Count == 0) return; // nothing to show — no message section

        view.Column([Card.Default, "p-0 overflow-hidden gap-0"], content: view =>
        {
            // Header
            view.Row(["px-4 py-3 border-b border-border items-center gap-2 bg-primary/5"], content: view =>
            {
                view.Box(["w-7 h-7 rounded-lg bg-primary/15 flex items-center justify-center shrink-0"],
                    content: v => v.Icon(["text-primary w-3.5 h-3.5"], name: "mail"));
                view.Column(["gap-0 flex-1"], content: view =>
                {
                    view.Row(["items-center gap-2"], content: view =>
                    {
                        view.Text(["text-sm font-semibold"], "Messages from Migri");
                        if (unread > 0)
                        {
                            view.Box(["min-w-[18px] h-[18px] rounded-full bg-error-primary flex items-center justify-center px-1"],
                                content: v => v.Text(["text-[10px] text-white font-bold"], unread.ToString()));
                        }
                    });
                    view.Text(["text-xs text-muted-foreground"],
                        "Official communications regarding your application");
                });
            });

            // Thread body
            view.Column(["px-4 py-4 gap-3 max-h-[380px] overflow-y-auto"], content: view =>
            {
                foreach (var msg in allVisible)
                    RenderApplicantThreadMessage(view, msg);

                // Mark as read (in render — safe since we only expand the list)
                if (unread > 0)
                    MarkThreadReadByApplicant(app.ApplicationId);
            });

            // Reply compose or prompt
            view.Column(["px-4 pb-4 gap-3 border-t border-border pt-3"], content: view =>
            {
                if (_showReplyCompose.Value)
                {
                    RenderApplicantReplyCompose(view, app);
                }
                else
                {
                    // Show reply button only if there's a supplement request or a general message
                    bool canReply = officerMsgs.Any(m =>
                        m.Type is PermitReady.OfficialMessageType.SupplementRequest
                               or PermitReady.OfficialMessageType.GeneralMessage);

                    if (canReply)
                    {
                        view.Button([Button.OutlineMd, "w-full gap-2"],
                            content: v =>
                            {
                                v.Icon(["w-4 h-4"], name: "reply");
                                v.Text([], "Reply to Migri");
                            },
                            onClick: async () =>
                            {
                                _showReplyCompose.Value = true;
                                _replyBody.Value        = "";
                                _replyAttachFileName.Value = "";
                            });
                    }
                }
            });
        });
    }

    private static void RenderApplicantThreadMessage(UIView view, PermitReady.OfficialMessage msg)
    {
        bool isApplicant = msg.SenderRole == "applicant";

        var (align, bubbleStyle, name, iconName) = isApplicant
            ? ("justify-end",  "bg-primary text-primary-foreground rounded-2xl rounded-br-sm max-w-[80%]",
               "You", "user")
            : ("justify-start", "bg-muted/60 border border-border rounded-2xl rounded-bl-sm max-w-[85%]",
               "Migri Officer", "shield-check");

        var typeLabel = msg.Type switch
        {
            PermitReady.OfficialMessageType.SupplementRequest  => "Supplement Request",
            PermitReady.OfficialMessageType.StatusNotification => "Status Update",
            PermitReady.OfficialMessageType.ApplicantReply     => "Your Reply",
            _                                                   => isApplicant ? "Your Message" : "Message from Migri",
        };

        view.Row([$"{align} gap-2 items-end"], content: view =>
        {
            if (!isApplicant)
            {
                view.Box(["w-6 h-6 rounded-full bg-primary flex items-center justify-center shrink-0 mb-0.5"],
                    content: v => v.Icon(["text-primary-foreground w-3 h-3"], name: iconName));
            }

            view.Column([$"{bubbleStyle} px-3.5 py-2.5 gap-0.5"], content: view =>
            {
                view.Row(["items-center justify-between gap-3 mb-0.5"], content: view =>
                {
                    view.Text(["text-[10px] font-semibold uppercase tracking-wide opacity-70"], typeLabel);
                    view.Text(["text-[10px] opacity-60"], msg.SentAt.ToString("dd MMM HH:mm"));
                });

                view.Text(["text-sm leading-relaxed whitespace-pre-wrap"], msg.Content);

                if (!string.IsNullOrEmpty(msg.AttachmentFileName))
                {
                    view.Row(["items-center gap-1.5 mt-1.5 bg-black/10 rounded-md px-2 py-1"], content: view =>
                    {
                        view.Icon(["w-3 h-3 shrink-0"], name: "paperclip");
                        view.Text(["text-xs font-mono truncate"], msg.AttachmentFileName);
                    });
                }
            });

            if (isApplicant)
            {
                view.Box(["w-6 h-6 rounded-full bg-muted border border-border flex items-center justify-center shrink-0 mb-0.5"],
                    content: v => v.Icon(["text-muted-foreground w-3 h-3"], name: "user"));
            }
        });
    }

    // ── Reply compose ─────────────────────────────────────────────────────────

    private void RenderApplicantReplyCompose(UIView view, PermitReady.StoredApplication app)
    {
        var hasSupplementRequest = _officialMessages.Value.Any(m =>
            m.ApplicationId == app.ApplicationId &&
            m.Type == PermitReady.OfficialMessageType.SupplementRequest);

        view.Column(["gap-3"], content: view =>
        {
            // Hint text
            if (hasSupplementRequest)
            {
                view.Row([Alert.Warning, "rounded-lg border px-3 py-2.5 items-start gap-2"], content: view =>
                {
                    view.Icon(["text-warning-primary w-3.5 h-3.5 mt-0.5 shrink-0"], name: "alert-circle");
                    view.Text(["text-xs text-warning-primary leading-relaxed"],
                        "Describe the documents you are submitting and attach your PDF files.");
                });
            }

            // Text area
            view.TextArea(
                [Input.Default, "resize-none min-h-[80px] text-sm"],
                placeholder: hasSupplementRequest
                    ? "Describe your response to the supplement request. List the documents you are attaching and any relevant explanations..."
                    : "Write your message to Migri...",
                value: _replyBody.Value,
                onValueChange: async v => { _replyBody.Value = v; });

            // Optional file attachment
            if (!string.IsNullOrEmpty(_replyAttachFileName.Value))
            {
                view.Row(["items-center gap-2 bg-success-primary/10 border border-success-primary/30 rounded-md px-3 py-2"], content: view =>
                {
                    view.Icon(["text-success-primary w-3.5 h-3.5 shrink-0"], name: "file-check");
                    view.Text(["text-xs font-mono flex-1 truncate text-success-primary"], _replyAttachFileName.Value);
                    view.Button([Button.GhostSm, "w-5 h-5 p-0 rounded-full text-muted-foreground"],
                        content: v => v.Icon(["w-3 h-3"], name: "x"),
                        onClick: async () =>
                        {
                            _replyAttachFileName.Value = "";
                        });
                });
            }
            else
            {
                view.FileUploadZone(
                    accept: [".pdf"],
                    maxFileSize: 5 * 1024 * 1024,
                    onUploadComplete: async args =>
                    {
                        _replyAttachFileName.Value = args.FileName;
                        await CacheUploadedDocBytesAsync(args.FileName, args.LocalTempFilePath!);
                    },
                    onDragActiveChange: async d => { _dragReplyFile.Value = d; },
                    zoneStyle: _dragReplyFile.Value
                        ? [FileUpload.Zone.Compact, FileUpload.Zone.Active]
                        : [FileUpload.Zone.Compact],
                    activeStyle: [FileUpload.Zone.Active],
                    content: v =>
                    {
                        v.Icon([FileUpload.Icon.Base, "w-4 h-4 mb-0.5"], name: "paperclip");
                        v.Text(["text-xs font-medium"], "Attach document (optional)");
                        v.Text(["text-[11px] text-muted-foreground"], "PDF · max 5 MB");
                    });
            }

            // Action buttons
            view.Row(["gap-2 justify-end"], content: view =>
            {
                view.Button([Button.OutlineMd],
                    "Cancel",
                    onClick: async () =>
                    {
                        _showReplyCompose.Value    = false;
                        _replyBody.Value           = "";
                        _replyAttachFileName.Value = "";
                    });

                view.Button([Button.PrimaryMd, "gap-2"],
                    content: v =>
                    {
                        v.Icon(["w-4 h-4"], name: "send");
                        v.Text([], "Send Reply");
                    },
                    disabled: string.IsNullOrWhiteSpace(_replyBody.Value),
                    onClick: async () =>
                    {
                        var body       = _replyBody.Value.Trim();
                        var attachment = string.IsNullOrWhiteSpace(_replyAttachFileName.Value)
                            ? null : _replyAttachFileName.Value;

                        if (string.IsNullOrEmpty(body)) return;

                        PostOfficialMessage(app.ApplicationId,
                            PermitReady.OfficialMessageType.ApplicantReply,
                            "applicant", body, attachment);

                        // Save attachment to DB if uploaded
                        if (attachment != null && _uploadedDocBytes.Value.TryGetValue(attachment, out var bytes))
                            _ = PermitReady.PermitReadyDb.SaveDocumentAsync(_host, app.ApplicationId, attachment, bytes);

                        // Auto-advance status if responding to supplement request
                        if (app.Status == PermitReady.ApplicationStatus.SupplementRequested)
                            UpdateApplicationStatus(app.ApplicationId,
                                PermitReady.ApplicationStatus.SupplementReceived);

                        _showReplyCompose.Value    = false;
                        _replyBody.Value           = "";
                        _replyAttachFileName.Value = "";
                    });
            });
        });
    }
}
