public partial class IkonDemoApp
{
    // ── Countries list (top immigration source countries + comprehensive) ──
    private static readonly string[] Countries =
    [
        "Afghanistan", "Albania", "Algeria", "Argentina", "Armenia", "Australia",
        "Austria", "Azerbaijan", "Bangladesh", "Belarus", "Belgium", "Bolivia",
        "Bosnia and Herzegovina", "Brazil", "Bulgaria", "Cambodia", "Cameroon",
        "Canada", "Chile", "China", "Colombia", "Congo (DRC)", "Croatia",
        "Cuba", "Czech Republic", "Denmark", "Ecuador", "Egypt", "Estonia",
        "Ethiopia", "Finland", "France", "Georgia", "Germany", "Ghana",
        "Greece", "Guatemala", "Hungary", "India", "Indonesia", "Iran",
        "Iraq", "Ireland", "Israel", "Italy", "Japan", "Jordan", "Kazakhstan",
        "Kenya", "Kosovo", "Kyrgyzstan", "Latvia", "Lebanon", "Libya",
        "Lithuania", "Malaysia", "Mexico", "Moldova", "Mongolia", "Montenegro",
        "Morocco", "Myanmar", "Nepal", "Netherlands", "New Zealand", "Nigeria",
        "North Macedonia", "Norway", "Pakistan", "Palestine", "Peru",
        "Philippines", "Poland", "Portugal", "Romania", "Russia", "Rwanda",
        "Saudi Arabia", "Senegal", "Serbia", "Sierra Leone", "Slovakia",
        "Slovenia", "Somalia", "South Korea", "Spain", "Sri Lanka", "Sudan",
        "Sweden", "Switzerland", "Syria", "Tajikistan", "Tanzania", "Thailand",
        "Tunisia", "Turkey", "Turkmenistan", "Uganda", "Ukraine",
        "United Arab Emirates", "United Kingdom", "United States", "Uzbekistan",
        "Venezuela", "Vietnam", "Yemen", "Zambia", "Zimbabwe"
    ];

    // ── Categories per group ──────────────────────────────────────────────
    private static readonly PermitReady.PermitCategory[] WorkCategories =
    [
        PermitReady.PermitCategory.EmployeePermit,
        PermitReady.PermitCategory.SpecialistExpert,
        PermitReady.PermitCategory.EUBlueCard,
        PermitReady.PermitCategory.IntraCorporate,
        PermitReady.PermitCategory.SeasonalWorker,
        PermitReady.PermitCategory.Researcher,
        PermitReady.PermitCategory.SelfEmployed,
        PermitReady.PermitCategory.AuPair,
        PermitReady.PermitCategory.WorkingHoliday,
    ];

    private static readonly PermitReady.PermitCategory[] StudyCategories =
    [
        PermitReady.PermitCategory.StudentHigherEd,
        PermitReady.PermitCategory.StudentVocational,
        PermitReady.PermitCategory.LanguageCourse,
        PermitReady.PermitCategory.ExchangeStudent,
        PermitReady.PermitCategory.TraineeIntern,
    ];

    private static readonly PermitReady.PermitCategory[] FamilyCategories =
    [
        PermitReady.PermitCategory.SpouseOfFinnish,
        PermitReady.PermitCategory.SpouseOfEUCitizen,
        PermitReady.PermitCategory.SpouseOfPermitHolder,
        PermitReady.PermitCategory.ChildOfFinnish,
        PermitReady.PermitCategory.ChildOfPermitHolder,
        PermitReady.PermitCategory.ParentOfMinorFinnish,
        PermitReady.PermitCategory.OtherFamilyEU,
        PermitReady.PermitCategory.DependentFamily,
    ];

    private void RenderForm(UIView view)
    {
        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column([Container.Xl2, "py-8 px-4 gap-0 min-h-full"], content: view =>
            {
                // Page header
                view.Column(["gap-1 mb-6"], content: view =>
                {
                    view.Text([Text.H2], "Residence Permit Application");
                    view.Text(["text-sm text-muted-foreground"],
                        "Complete all sections carefully. Information must match your official documents exactly.");
                });

                // ── Section 1: Permit Type ────────────────────────────────
                FormSection(view, "1", "Permit Type", "Select the group and specific permit type you are applying for", "file-text", view =>
                {
                    // Group selection row
                    view.Row(["gap-4 flex-wrap mb-4"], content: view =>
                    {
                        PermitGroupCard(view, "Work",   "briefcase",      "Work-based permits",  "9 permit types");
                        PermitGroupCard(view, "Study",  "graduation-cap", "Study & training",    "5 permit types");
                        PermitGroupCard(view, "Family", "heart",          "Family reunification", "8 permit types");
                    });

                    // Sub-type grid for selected group
                    var activeCategories = _permitGroup.Value switch
                    {
                        "Work"   => WorkCategories,
                        "Study"  => StudyCategories,
                        "Family" => FamilyCategories,
                        _        => StudyCategories
                    };

                    view.Column(["gap-2"], content: view =>
                    {
                        view.Text(["text-xs font-medium text-muted-foreground uppercase tracking-wide"],
                            $"Select specific {_permitGroup.Value} permit type:");
                        view.Row(["flex-wrap gap-2"], content: view =>
                        {
                            foreach (var cat in activeCategories)
                            {
                                var c = cat; // capture
                                bool active = _permitCategory.Value == c.ToString();
                                view.Button(
                                    active
                                        ? [Button.PrimarySm, "h-auto py-2 px-3 text-left gap-1.5 items-center"]
                                        : [Button.OutlineSm, "h-auto py-2 px-3 text-left gap-1.5 items-center hover:border-primary/50"],
                                    onClick: async () =>
                                    {
                                        _permitCategory.Value = c.ToString();
                                    },
                                    content: v =>
                                    {
                                        v.Text(["text-xs font-medium"], c.ShortName());
                                    });
                            }
                        });

                        // Show description of selected category
                        if (Enum.TryParse<PermitReady.PermitCategory>(_permitCategory.Value, out var selectedCat))
                        {
                            view.Row(["items-center gap-2 mt-1 p-3 rounded-lg bg-primary/5 border border-primary/20"], content: view =>
                            {
                                view.Icon(["text-primary w-4 h-4 shrink-0"], name: "info");
                                view.Text(["text-xs text-primary font-medium"], selectedCat.DisplayName());
                            });
                        }
                    });
                });

                // ── Section 2: Personal Information ──────────────────────
                FormSection(view, "2", "Personal Information", "Must match your passport exactly", "user", view =>
                {
                    view.Row(["gap-4 flex-wrap"], content: view =>
                    {
                        view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                        {
                            view.Text([FormField.LabelRequired], "Full Name");
                            view.TextField([Input.Default], placeholder: "As printed on passport",
                                value: _fullName.Value, onValueChange: async v => { _fullName.Value = v; });
                            view.Text([FormField.HelpText], "Include all names — first, middle, last");
                        });

                        view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                        {
                            view.Text([FormField.LabelRequired], "Nationality");
                            view.Select(
                                options: Countries.Select(c => new SelectOption(c, c)).ToArray(),
                                value: _nationality.Value,
                                placeholder: "Select country...",
                                triggerStyle: [Select.Trigger, "w-full"],
                                onValueChange: async v => { _nationality.Value = v ?? ""; });
                        });
                    });

                    view.Row(["gap-4 flex-wrap"], content: view =>
                    {
                        view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                        {
                            view.Text([FormField.LabelRequired], "Email Address");
                            view.TextField([Input.Default], placeholder: "you@email.com",
                                value: _email.Value, onValueChange: async v => { _email.Value = v; });
                            view.Text([FormField.HelpText], "All correspondence will be sent here");
                        });

                        view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                        {
                            view.Text([FormField.Label], "Phone Number");
                            view.TextField([Input.Default], placeholder: "+358 40 123 4567",
                                value: _phone.Value, onValueChange: async v => { _phone.Value = v; });
                        });
                    });

                    view.Row(["gap-4 flex-wrap"], content: view =>
                    {
                        view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                        {
                            view.Text([FormField.Label], "Passport Number");
                            view.TextField([Input.Default, "font-mono tracking-widest uppercase"],
                                placeholder: "e.g. AB1234567",
                                value: _passportNumber.Value,
                                onValueChange: async v => { _passportNumber.Value = v.ToUpperInvariant(); });
                        });

                        view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                        {
                            view.Text([FormField.Label], "Passport Expiry Date");
                            view.TextField([Input.Default], placeholder: "YYYY-MM-DD",
                                value: _passportExpiry.Value,
                                onValueChange: async v => { _passportExpiry.Value = v; });
                            view.Text([FormField.HelpText], "Must be valid for full permit period + 6 months");

                            // Live expiry warning
                            if (DateTime.TryParse(_passportExpiry.Value, out var exp))
                            {
                                var days = (exp - DateTime.UtcNow).Days;
                                if (days < 180)
                                    view.Text([FormField.WarningText], days < 0
                                        ? "Passport has expired — renewal required"
                                        : $"Only {days} days remaining — may be insufficient");
                            }
                        });
                    });
                });

                // ── Section 3: Identity Document ─────────────────────────
                FormSection(view, "3", "Passport Copy", "Upload a scanned copy of your passport photo page", "shield-check", view =>
                {
                    DocUploadZone(view,
                        doc: _passportDoc.Value,
                        isDragging: _dragPassport.Value,
                        hint: "Drop passport PDF here or click to upload",
                        onDragChange: async d => { _dragPassport.Value = d; },
                        onUploaded: async args =>
                        {
                            _passportDoc.Value = new PermitReady.UploadedDoc(
                                args.FileName, args.Size,
                                PermitReady.DocStatus.Verifying, [], []);
                            await VerifyPassportAsync(args.LocalTempFilePath!, args.FileName);
                        },
                        onRemove: async () => { _passportDoc.Value = null; });
                });

                // ── Section 4: Study / Work / Family Details ──────────────────────
                if (_permitGroup.Value == "Study")
                {
                    FormSection(view, "4", "Study Details", "Information about your studies in Finland", "graduation-cap", view =>
                    {
                        view.Row(["gap-4 flex-wrap"], content: view =>
                        {
                            view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                            {
                                view.Text([FormField.LabelRequired], "University / Institution");
                                view.TextField([Input.Default], placeholder: "e.g. University of Helsinki",
                                    value: _universityName.Value,
                                    onValueChange: async v => { _universityName.Value = v; });
                            });

                            view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                            {
                                view.Text([FormField.LabelRequired], "Degree Programme");
                                view.TextField([Input.Default], placeholder: "e.g. MSc Computer Science",
                                    value: _programName.Value,
                                    onValueChange: async v => { _programName.Value = v; });
                            });
                        });

                        view.Row(["gap-4 flex-wrap"], content: view =>
                        {
                            view.Column([FormField.Root, "flex-1 min-w-[180px]"], content: view =>
                            {
                                view.Text([FormField.LabelRequired], "Monthly Funds (€)");
                                view.TextField([Input.Default], placeholder: "e.g. 800",
                                    value: _fundsAmount.Value,
                                    onValueChange: async v => { _fundsAmount.Value = v; });
                                view.Text([FormField.HelpText], "Minimum €560/month (€6,720/year)");
                                if (decimal.TryParse(_fundsAmount.Value, out var f) && f > 0 && f < 560)
                                    view.Text([FormField.ErrorText], $"€{f:N0}/month is below the required minimum of €560/month");
                            });

                            view.Column([FormField.Root, "flex-1 min-w-[180px]"], content: view =>
                            {
                                view.Text([FormField.Label], "Studies Start Date");
                                view.TextField([Input.Default], placeholder: "YYYY-MM-DD",
                                    value: _studyStartDate.Value,
                                    onValueChange: async v => { _studyStartDate.Value = v; });
                            });
                        });
                    });

                    // ── Section 5: Student Documents ─────────────────────
                    FormSection(view, "5", "Supporting Documents", "Upload all required documents as PDF (max 1 MB each)", "paperclip", view =>
                    {
                        view.Row(["gap-5 flex-wrap"], content: view =>
                        {
                            DocSlot(view, "Acceptance Letter *", "acceptance",
                                "Official letter from the Finnish institution confirming your enrolment",
                                _acceptanceDoc.Value, _dragAcceptance.Value,
                                async d => { _dragAcceptance.Value = d; },
                                async args =>
                                {
                                    _acceptanceDoc.Value = new PermitReady.UploadedDoc(args.FileName, args.Size, PermitReady.DocStatus.Verifying, [], []);
                                    await VerifyGenericDocAsync(args.LocalTempFilePath!, args.FileName, "acceptance letter or enrolment confirmation",
                                        d => _acceptanceDoc.Value = d, () => _acceptanceDoc.Value);
                                },
                                async () => { _acceptanceDoc.Value = null; });

                            DocSlot(view, "Academic Transcript *", "transcript",
                                "Official transcripts from your previous institution",
                                _transcriptDoc.Value, _dragTranscript.Value,
                                async d => { _dragTranscript.Value = d; },
                                async args =>
                                {
                                    _transcriptDoc.Value = new PermitReady.UploadedDoc(args.FileName, args.Size, PermitReady.DocStatus.Verifying, [], []);
                                    await VerifyGenericDocAsync(args.LocalTempFilePath!, args.FileName, "academic transcript or grade report",
                                        d => _transcriptDoc.Value = d, () => _transcriptDoc.Value);
                                },
                                async () => { _transcriptDoc.Value = null; });

                            DocSlot(view, "Bank Statement *", "bank-statement",
                                "Last 3 months statements showing sufficient funds",
                                _bankStatementDoc.Value, _dragBankStatement.Value,
                                async d => { _dragBankStatement.Value = d; },
                                async args =>
                                {
                                    _bankStatementDoc.Value = new PermitReady.UploadedDoc(args.FileName, args.Size, PermitReady.DocStatus.Verifying, [], []);
                                    decimal? claimed = decimal.TryParse(_fundsAmount.Value, out var fv) ? fv : null;
                                    await VerifyBankStatementAsync(args.LocalTempFilePath!, args.FileName, claimed);
                                },
                                async () => { _bankStatementDoc.Value = null; });
                        });
                    });
                }
                else if (_permitGroup.Value == "Work")
                {
                    FormSection(view, "4", "Employment Details", "Information about your job in Finland", "briefcase", view =>
                    {
                        view.Row(["gap-4 flex-wrap"], content: view =>
                        {
                            view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                            {
                                view.Text([FormField.LabelRequired], "Employer Name");
                                view.TextField([Input.Default], placeholder: "Legal company name, e.g. Nokia Oyj",
                                    value: _employerName.Value,
                                    onValueChange: async v => { _employerName.Value = v; });
                                view.Text([FormField.HelpText], "Use the exact registered company name");
                            });

                            view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                            {
                                view.Text([FormField.LabelRequired], "Job Title");
                                view.TextField([Input.Default], placeholder: "As stated in your contract",
                                    value: _jobTitle.Value,
                                    onValueChange: async v => { _jobTitle.Value = v; });
                            });
                        });

                        view.Row(["gap-4 flex-wrap"], content: view =>
                        {
                            view.Column([FormField.Root, "flex-1 min-w-[180px]"], content: view =>
                            {
                                view.Text([FormField.LabelRequired], "Monthly Gross Salary (€)");
                                view.TextField([Input.Default], placeholder: "e.g. 3500",
                                    value: _salaryAmount.Value,
                                    onValueChange: async v => { _salaryAmount.Value = v; });
                                view.Text([FormField.HelpText], "Minimum €1,500/month gross");
                                if (decimal.TryParse(_salaryAmount.Value, out var s) && s > 0 && s < 1500)
                                    view.Text([FormField.ErrorText], $"€{s:N0}/month is below the minimum of €1,500/month");
                            });

                            view.Column([FormField.Root, "flex-1 min-w-[180px]"], content: view =>
                            {
                                view.Text([FormField.Label], "Contract Reference No.");
                                view.TextField([Input.Default, "font-mono"], placeholder: "e.g. NOK-2024-789",
                                    value: _contractRef.Value,
                                    onValueChange: async v => { _contractRef.Value = v; });
                            });

                            view.Column([FormField.Root, "flex-1 min-w-[180px]"], content: view =>
                            {
                                view.Text([FormField.Label], "Employment Start Date");
                                view.TextField([Input.Default], placeholder: "YYYY-MM-DD",
                                    value: _workStartDate.Value,
                                    onValueChange: async v => { _workStartDate.Value = v; });
                            });
                        });
                    });

                    // ── Section 5: Work Documents ─────────────────────────
                    FormSection(view, "5", "Supporting Documents", "Upload all required documents as PDF (max 1 MB each)", "paperclip", view =>
                    {
                        view.Row(["gap-5 flex-wrap"], content: view =>
                        {
                            DocSlot(view, "Employment Contract *", "employment-contract",
                                "Signed contract showing role, salary, and start date",
                                _contractDoc.Value, _dragContract.Value,
                                async d => { _dragContract.Value = d; },
                                async args =>
                                {
                                    _contractDoc.Value = new PermitReady.UploadedDoc(args.FileName, args.Size, PermitReady.DocStatus.Verifying, [], []);
                                    await VerifyGenericDocAsync(args.LocalTempFilePath!, args.FileName, "employment contract or job offer letter",
                                        d => _contractDoc.Value = d, () => _contractDoc.Value);
                                },
                                async () => { _contractDoc.Value = null; });

                            DocSlot(view, "Salary Proof / Payslips", "salary-proof",
                                "Recent payslips or payroll confirmation if already employed",
                                _salaryProofDoc.Value, _dragSalaryProof.Value,
                                async d => { _dragSalaryProof.Value = d; },
                                async args =>
                                {
                                    _salaryProofDoc.Value = new PermitReady.UploadedDoc(args.FileName, args.Size, PermitReady.DocStatus.Verifying, [], []);
                                    decimal? claimed = decimal.TryParse(_salaryAmount.Value, out var sv) ? sv : null;
                                    await VerifyBankStatementAsync(args.LocalTempFilePath!, args.FileName, claimed);
                                },
                                async () => { _salaryProofDoc.Value = null; });
                        });
                    });
                }
                else // Family
                {
                    FormSection(view, "4", "Family / Sponsor Details", "Information about your sponsor or family relationship in Finland", "heart", view =>
                    {
                        view.Row(["gap-4 flex-wrap"], content: view =>
                        {
                            view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                            {
                                view.Text([FormField.LabelRequired], "Sponsor / Family Member Name");
                                view.TextField([Input.Default], placeholder: "Full legal name of your sponsor",
                                    value: _sponsorName.Value,
                                    onValueChange: async v => { _sponsorName.Value = v; });
                                view.Text([FormField.HelpText], "The person you are joining in Finland");
                            });

                            view.Column([FormField.Root, "flex-1 min-w-[200px]"], content: view =>
                            {
                                view.Text([FormField.Label], "Sponsor's Permit / ID Number");
                                view.TextField([Input.Default, "font-mono"], placeholder: "e.g. FI-RP-2024-1234",
                                    value: _sponsorPermitNumber.Value,
                                    onValueChange: async v => { _sponsorPermitNumber.Value = v; });
                                view.Text([FormField.HelpText], "Residence permit number or Finnish personal ID");
                            });
                        });
                    });

                    // ── Section 5: Family Documents ───────────────────────
                    FormSection(view, "5", "Supporting Documents", "Upload all required documents as PDF (max 1 MB each)", "paperclip", view =>
                    {
                        view.Row(["gap-5 flex-wrap"], content: view =>
                        {
                            DocSlot(view, "Relationship Document *", "relationship-doc",
                                "Marriage certificate, birth certificate, or other proof of relationship",
                                _acceptanceDoc.Value, _dragAcceptance.Value,
                                async d => { _dragAcceptance.Value = d; },
                                async args =>
                                {
                                    _acceptanceDoc.Value = new PermitReady.UploadedDoc(args.FileName, args.Size, PermitReady.DocStatus.Verifying, [], []);
                                    await VerifyGenericDocAsync(args.LocalTempFilePath!, args.FileName, "marriage certificate, birth certificate, or relationship proof",
                                        d => _acceptanceDoc.Value = d, () => _acceptanceDoc.Value);
                                },
                                async () => { _acceptanceDoc.Value = null; });

                            DocSlot(view, "Sponsor's Permit Copy", "sponsor-permit",
                                "Copy of sponsor's Finnish residence permit or citizenship",
                                _contractDoc.Value, _dragContract.Value,
                                async d => { _dragContract.Value = d; },
                                async args =>
                                {
                                    _contractDoc.Value = new PermitReady.UploadedDoc(args.FileName, args.Size, PermitReady.DocStatus.Verifying, [], []);
                                    await VerifyGenericDocAsync(args.LocalTempFilePath!, args.FileName, "residence permit or identity document",
                                        d => _contractDoc.Value = d, () => _contractDoc.Value);
                                },
                                async () => { _contractDoc.Value = null; });
                        });
                    });
                }

                // Error message
                if (!string.IsNullOrEmpty(_formError.Value))
                {
                    view.Box([Alert.Danger, "px-4 py-3 rounded-lg border mt-2"], content: view =>
                        view.Text([], _formError.Value));
                }

                // Submit button
                view.Button(
                    [Button.PrimaryMd, "w-full mt-4 mb-8"],
                    _isSubmitting.Value ? "Checking Application..." : "Review My Application →",
                    disabled: _isSubmitting.Value,
                    onClick: async () => { await SubmitApplicationAsync(); });
            });
        });
    }

    // ── Section wrapper ───────────────────────────────────────────────────

    private static void FormSection(UIView view, string num, string title, string subtitle, string icon, Action<UIView> content)
    {
        view.Column([Card.Default, "p-6 gap-5 mb-4"], content: view =>
        {
            view.Row(["items-start gap-3"], content: view =>
            {
                view.Box(["w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0 mt-0.5"], content: view =>
                    view.Text(["text-primary text-xs font-bold"], num));
                view.Column(["gap-0.5 flex-1"], content: view =>
                {
                    view.Row(["items-center gap-2"], content: view =>
                    {
                        view.Icon(["text-muted-foreground w-4 h-4"], name: icon);
                        view.Text(["font-semibold text-base"], title);
                    });
                    view.Text(["text-xs text-muted-foreground"], subtitle);
                });
            });

            view.Separator(["my-1"]);

            content(view);
        });
    }

    // ── Permit group card ─────────────────────────────────────────────────

    private void PermitGroupCard(UIView view, string group, string icon, string desc, string subtitle)
    {
        bool active = _permitGroup.Value == group;
        view.Button(
            active
                ? [Card.Default, "flex-1 min-w-[160px] p-4 border-2 border-primary bg-primary/5 text-left cursor-pointer flex flex-col gap-1.5"]
                : [Card.Default, "flex-1 min-w-[160px] p-4 text-left cursor-pointer flex flex-col gap-1.5 hover:border-primary/50"],
            onClick: async () =>
            {
                _permitGroup.Value = group;
                // Set default category for the group
                _permitCategory.Value = group switch
                {
                    "Work"   => PermitReady.PermitCategory.EmployeePermit.ToString(),
                    "Study"  => PermitReady.PermitCategory.StudentHigherEd.ToString(),
                    "Family" => PermitReady.PermitCategory.SpouseOfFinnish.ToString(),
                    _        => PermitReady.PermitCategory.StudentHigherEd.ToString()
                };
            },
            content: view =>
            {
                view.Row(["items-center gap-2.5"], content: view =>
                {
                    view.Box([$"{(active ? "bg-primary" : "bg-muted")} w-9 h-9 rounded-lg flex items-center justify-center shrink-0"], content: view =>
                        view.Icon([$"{(active ? "text-primary-foreground" : "text-muted-foreground")} w-4 h-4"], name: icon));
                    view.Column(["gap-0"], content: view =>
                    {
                        view.Text(["font-semibold text-sm"], group);
                        view.Text(["text-xs text-muted-foreground"], subtitle);
                    });
                });
                view.Text(["text-xs text-muted-foreground"], desc);
                if (active)
                    view.Box([Badge.DefaultSm, "w-fit"], content: v => v.Text([], "Selected"));
            });
    }

    // ── Document upload zone ──────────────────────────────────────────────

    private static void DocUploadZone(UIView view,
        PermitReady.UploadedDoc? doc,
        bool isDragging,
        string hint,
        Func<bool, Task> onDragChange,
        Func<FileUploadCompleteArgs, Task> onUploaded,
        Func<Task> onRemove)
    {
        if (doc == null)
        {
            // Empty state — show upload zone
            view.FileUploadZone(
                accept: [".pdf"],
                maxFileSize: 1 * 1024 * 1024,
                onUploadComplete: async args => { await onUploaded(args); },
                onDragActiveChange: async d => { await onDragChange(d); },
                zoneStyle: isDragging
                    ? [FileUpload.Zone.Documents, FileUpload.Zone.Active]
                    : [FileUpload.Zone.Documents],
                activeStyle: [FileUpload.Zone.Active],
                content: v =>
                {
                    v.Icon([FileUpload.Icon.Error, "mb-2"], name: "file-text");
                    v.Text(["text-sm font-medium"], hint);
                    v.Text(["text-xs text-muted-foreground mt-1"], "PDF only · Max 1 MB");
                });
        }
        else
        {
            // File uploaded — show status card
            DocStatusCard(view, doc, onRemove);
        }
    }

    // ── Document slot (compact, for the supporting docs grid) ────────────

    private static void DocSlot(UIView view,
        string label, string slotName,
        string description,
        PermitReady.UploadedDoc? doc,
        bool isDragging,
        Func<bool, Task> onDragChange,
        Func<FileUploadCompleteArgs, Task> onUploaded,
        Func<Task> onRemove)
    {
        view.Column(["flex-1 min-w-[260px] gap-2"], content: view =>
        {
            view.Text([FormField.Label], label);
            view.Text(["text-xs text-muted-foreground -mt-1.5"], description);

            if (doc == null)
            {
                view.FileUploadZone(
                    accept: [".pdf"],
                    maxFileSize: 1 * 1024 * 1024,
                    onUploadComplete: async args => { await onUploaded(args); },
                    onDragActiveChange: async d => { await onDragChange(d); },
                    zoneStyle: isDragging
                        ? [FileUpload.Zone.Compact, FileUpload.Zone.Active]
                        : [FileUpload.Zone.Compact],
                    activeStyle: [FileUpload.Zone.Active],
                    content: v =>
                    {
                        v.Icon([FileUpload.Icon.Base, "mb-1 w-6 h-6"], name: "upload");
                        v.Text(["text-xs font-medium"], "Drop PDF here or click");
                        v.Text(["text-xs text-muted-foreground"], "Max 1 MB");
                    });
            }
            else
            {
                DocStatusCard(view, doc, onRemove);
            }
        });
    }

    // ── Uploaded file status card ─────────────────────────────────────────

    private static void DocStatusCard(UIView view, PermitReady.UploadedDoc doc, Func<Task> onRemove)
    {
        var (borderColor, bgColor, statusIcon, statusLabel) = doc.Status switch
        {
            PermitReady.DocStatus.Verifying => ("border-border",  "bg-muted/30",           "loader-2",     "Verifying..."),
            PermitReady.DocStatus.Verified  => ("border-success", "bg-success-primary/10", "check-circle", "Verified"),
            PermitReady.DocStatus.Warning   => ("border-warning", "bg-warning-primary/10", "alert-circle", "Warning"),
            PermitReady.DocStatus.Failed    => ("border-error",   "bg-error-primary/10",   "x-circle",     "Failed"),
            _                               => ("border-border",  "bg-muted/20",           "file",         "Uploaded"),
        };

        var statusColor = doc.Status switch
        {
            PermitReady.DocStatus.Verified => "text-success-primary",
            PermitReady.DocStatus.Warning  => "text-warning-primary",
            PermitReady.DocStatus.Failed   => "text-error-primary",
            _                              => "text-muted-foreground",
        };

        view.Column([$"border {borderColor} {bgColor} rounded-lg p-3 gap-2"], content: view =>
        {
            view.Row(["items-center gap-2"], content: view =>
            {
                view.Icon([$"{statusColor} w-4 h-4 shrink-0",
                    doc.Status == PermitReady.DocStatus.Verifying ? "animate-spin" : ""],
                    name: statusIcon);

                view.Column(["flex-1 min-w-0 gap-0"], content: view =>
                {
                    view.Text(["text-xs font-medium truncate"], doc.FileName);
                    view.Text(["text-xs text-muted-foreground"], $"{doc.SizeLabel} · {statusLabel}");
                });

                view.Button([Button.GhostSm, Button.Size.Icon, "shrink-0 w-6 h-6"],
                    content: v => v.Icon(["w-3 h-3 text-muted-foreground"], name: "x"),
                    onClick: async () => { await onRemove(); });
            });

            // Alerts from verification
            foreach (var alert in doc.Alerts)
            {
                var alertStyle = doc.Status == PermitReady.DocStatus.Verified
                    ? "text-success-primary bg-success-primary/10"
                    : doc.Status == PermitReady.DocStatus.Warning
                        ? "text-warning-primary bg-warning-primary/10"
                        : "text-error-primary bg-error-primary/10";

                view.Row([$"{alertStyle} rounded px-2 py-1.5 gap-1.5 items-start"], content: view =>
                {
                    view.Icon(["w-3.5 h-3.5 shrink-0 mt-0.5"], name:
                        doc.Status == PermitReady.DocStatus.Verified ? "check" :
                        doc.Status == PermitReady.DocStatus.Warning  ? "alert-triangle" : "x");
                    view.Text(["text-xs leading-relaxed"], alert);
                });
            }

            // Show extracted fields if verified
            if (doc.Status == PermitReady.DocStatus.Verified && doc.Extracted.Count > 0)
            {
                view.Row(["gap-3 flex-wrap mt-1"], content: view =>
                {
                    foreach (var (key, val) in doc.Extracted)
                        view.Box(["text-xs bg-muted rounded px-2 py-0.5 text-muted-foreground"], content: v =>
                            v.Text([], $"{key}: {val}"));
                });
            }
        });
    }
}
