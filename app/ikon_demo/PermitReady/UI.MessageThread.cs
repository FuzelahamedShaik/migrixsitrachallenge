public partial class IkonDemoApp
{
    // ── Officer-side message thread (right panel, "Messages" tab) ────────────

    private void RenderMessageThread(UIView view, PermitReady.StoredApplication app)
    {
        var appMsgs = _officialMessages.Value
            .Where(m => m.ApplicationId == app.ApplicationId)
            .OrderBy(m => m.SentAt)
            .ToList();

        // Only visible messages (skip internal profile-access notices in thread view)
        var visibleMsgs = appMsgs
            .Where(m => m.Type != PermitReady.OfficialMessageType.ProfileAccessNotice)
            .ToList();

        var unread = appMsgs.Count(m => m.SenderRole == "applicant" && !m.ReadByOfficer);

        view.Column(["flex-1 h-full flex flex-col overflow-hidden"], content: view =>
        {
            // Thread header
            view.Row(["px-5 py-3 border-b border-border bg-background items-center justify-between shrink-0"], content: view =>
            {
                view.Row(["items-center gap-2"], content: view =>
                {
                    view.Box(["w-7 h-7 rounded-lg bg-primary/10 flex items-center justify-center"],
                        content: v => v.Icon(["text-primary w-3.5 h-3.5"], name: "mail"));
                    view.Column(["gap-0"], content: view =>
                    {
                        view.Row(["items-center gap-2"], content: view =>
                        {
                            view.Text(["text-sm font-semibold"], "Official Communications");
                            if (unread > 0)
                            {
                                view.Box(["min-w-[18px] h-[18px] rounded-full bg-error-primary flex items-center justify-center px-1"],
                                    content: v => v.Text(["text-[10px] text-white font-bold"], unread.ToString()));
                            }
                        });
                        view.Text(["text-xs text-muted-foreground"],
                            $"{visibleMsgs.Count} message{(visibleMsgs.Count != 1 ? "s" : "")} · {app.ApplicationId}");
                    });
                });

                view.Button(
                    _showComposeMessage.Value
                        ? [Button.PrimarySm, "gap-1 text-xs"]
                        : [Button.OutlineSm, "gap-1 text-xs"],
                    content: v =>
                    {
                        v.Icon(["w-3.5 h-3.5"], name: _showComposeMessage.Value ? "x" : "edit-3");
                        v.Text([], _showComposeMessage.Value ? "Cancel" : "Compose");
                    },
                    onClick: async () =>
                    {
                        var closing = _showComposeMessage.Value;
                        _showComposeMessage.Value = !closing;
                        // Only reset body when explicitly cancelling (not when opened via draft card)
                        if (closing)
                        {
                            _composeMessageBody.Value = "";
                            _composeMessageType.Value = "GeneralMessage";
                        }
                    });
            });

            // Compose panel (slides in when active)
            if (_showComposeMessage.Value)
                RenderComposePanel(view, app);

            // Message list
            view.ScrollArea(
                rootStyle: ["flex-1 min-h-0"],
                autoScroll: true,
                autoScrollKey: visibleMsgs.Count.ToString(),
                content: view =>
                {
                    view.Column(["px-5 py-5 gap-4 min-h-full"], content: view =>
                    {
                        if (visibleMsgs.Count == 0)
                        {
                            view.Column(["items-center py-12 gap-3"], content: view =>
                            {
                                view.Box(["w-12 h-12 rounded-full bg-muted flex items-center justify-center"],
                                    content: v => v.Icon(["text-muted-foreground w-5 h-5"], name: "mail-open"));
                                view.Text(["text-sm text-muted-foreground font-medium"], "No messages yet");
                                view.Text(["text-xs text-muted-foreground text-center max-w-[220px]"],
                                    "Use Compose to send supplement requests or communicate with the applicant.");
                            });
                            return;
                        }

                        foreach (var msg in visibleMsgs)
                            RenderOfficerThreadMessage(view, msg);
                    });
                });
        });
    }

    private static void RenderOfficerThreadMessage(UIView view, PermitReady.OfficialMessage msg)
    {
        bool isApplicant = msg.SenderRole == "applicant";
        bool isSystem    = msg.SenderRole == "system";

        if (isSystem)
        {
            // System notice — centered small badge
            view.Column(["items-center gap-1.5"], content: view =>
            {
                view.Row(["items-center gap-1.5 bg-muted/60 border border-border/50 rounded-full px-3 py-1"],
                    content: view =>
                    {
                        view.Icon(["text-muted-foreground w-3 h-3"], name: "info");
                        view.Text(["text-[11px] text-muted-foreground italic"], msg.Content);
                    });
                view.Text(["text-[10px] text-muted-foreground"], msg.SentAt.ToString("dd MMM HH:mm"));
            });
            return;
        }

        var (align, bubbleStyle, avatarIcon, timeColor, typeLabelStyle) = isApplicant
            ? ("justify-start", "bg-background border border-border rounded-2xl rounded-bl-sm shadow-sm", "user", "text-muted-foreground", "text-success-primary")
            : ("justify-end",   "bg-primary text-primary-foreground rounded-2xl rounded-br-sm",           "shield", "text-primary-foreground/60", "text-primary-foreground/80");

        // Type label
        var typeLabel = msg.Type switch
        {
            PermitReady.OfficialMessageType.SupplementRequest => "Supplement Request",
            PermitReady.OfficialMessageType.ApplicantReply    => "Applicant Reply",
            _                                                  => isApplicant ? "Applicant Message" : "Migri Officer",
        };

        view.Row([$"{align} gap-2 items-end"], content: view =>
        {
            if (isApplicant)
            {
                view.Box(["w-7 h-7 rounded-full bg-muted border border-border flex items-center justify-center shrink-0 mb-0.5"],
                    content: v => v.Icon(["text-muted-foreground w-3.5 h-3.5"], name: avatarIcon));
            }

            view.Column([$"{bubbleStyle} px-4 py-3 max-w-[82%] gap-1"], content: view =>
            {
                // Type chip + timestamp
                view.Row(["items-center gap-2 justify-between mb-0.5"], content: view =>
                {
                    view.Text([$"text-[10px] font-semibold uppercase tracking-wide {typeLabelStyle}"], typeLabel);
                    view.Text([$"text-[10px] {timeColor}"], msg.SentAt.ToString("dd MMM HH:mm"));
                });

                view.Text(["text-sm leading-relaxed whitespace-pre-wrap"], msg.Content);

                if (!string.IsNullOrEmpty(msg.AttachmentFileName))
                {
                    view.Row(["items-center gap-1.5 mt-1.5 bg-black/10 rounded-md px-2.5 py-1.5"], content: view =>
                    {
                        view.Icon(["w-3 h-3 shrink-0"], name: "paperclip");
                        view.Text(["text-xs font-mono truncate"], msg.AttachmentFileName);
                    });
                }

                // Unread dot for officer (applicant replies not yet read)
                if (isApplicant && !msg.ReadByOfficer)
                {
                    view.Row(["items-center gap-1 mt-0.5"], content: view =>
                    {
                        view.Box(["w-2 h-2 rounded-full bg-error-primary"]);
                        view.Text(["text-[10px] text-error-primary font-medium"], "New");
                    });
                }
            });

            if (!isApplicant)
            {
                view.Box(["w-7 h-7 rounded-full bg-primary/80 flex items-center justify-center shrink-0 mb-0.5"],
                    content: v => v.Icon(["text-primary-foreground w-3.5 h-3.5"], name: "shield-check"));
            }
        });
    }

    // ── Officer compose panel ─────────────────────────────────────────────────

    private void RenderComposePanel(UIView view, PermitReady.StoredApplication app)
    {
        view.Column(["shrink-0 border-b border-border bg-background px-5 py-4 gap-3"], content: view =>
        {
            // Type selector
            view.Row(["gap-2 items-center"], content: view =>
            {
                view.Text(["text-xs font-medium text-muted-foreground shrink-0"], "Type:");
                foreach (var (val, label, icon) in new[] {
                    ("GeneralMessage",    "General",          "message-circle"),
                    ("SupplementRequest", "Supplement Request","alert-circle"),
                })
                {
                    var v = val;
                    view.Button(
                        _composeMessageType.Value == v
                            ? [Button.PrimarySm, "gap-1 text-xs rounded-full"]
                            : [Button.OutlineSm, "gap-1 text-xs rounded-full text-muted-foreground"],
                        content: btn =>
                        {
                            btn.Icon(["w-3 h-3"], name: icon);
                            btn.Text([], label);
                        },
                        onClick: async () => { _composeMessageType.Value = v; });
                }
            });

            // Message body
            var placeholder = _composeMessageType.Value == "SupplementRequest"
                ? "Describe the required documents or information. Be specific about what is needed and why. The applicant will receive this as an official supplement request."
                : "Write your official message to the applicant. This will appear in their application tracking page.";

            view.TextArea(
                [Input.Default, "resize-none min-h-[100px] text-sm"],
                placeholder: placeholder,
                value: _composeMessageBody.Value,
                onValueChange: async v => { _composeMessageBody.Value = v; });

            // Footer: send
            view.Row(["justify-end gap-2"], content: view =>
            {
                view.Button([Button.PrimaryMd, "gap-2"],
                    content: v =>
                    {
                        v.Icon(["w-4 h-4"], name: "send");
                        v.Text([], _composeMessageType.Value == "SupplementRequest" ? "Send Supplement Request" : "Send Message");
                    },
                    disabled: string.IsNullOrWhiteSpace(_composeMessageBody.Value),
                    onClick: async () =>
                    {
                        var body = _composeMessageBody.Value.Trim();
                        if (string.IsNullOrEmpty(body)) return;

                        var msgType = _composeMessageType.Value == "SupplementRequest"
                            ? PermitReady.OfficialMessageType.SupplementRequest
                            : PermitReady.OfficialMessageType.GeneralMessage;

                        PostOfficialMessage(app.ApplicationId, msgType, "officer", body);

                        // Supplement request also updates the application status
                        if (msgType == PermitReady.OfficialMessageType.SupplementRequest &&
                            app.Status is PermitReady.ApplicationStatus.UnderReview
                                       or PermitReady.ApplicationStatus.Screened)
                        {
                            UpdateApplicationStatus(app.ApplicationId,
                                PermitReady.ApplicationStatus.SupplementRequested, body);
                        }

                        _composeMessageBody.Value = "";
                        _showComposeMessage.Value = false;
                    });
            });
        });
    }
}
