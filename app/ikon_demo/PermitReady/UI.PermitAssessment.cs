public partial class IkonDemoApp
{
    // ── Assessment state ──────────────────────────────────────────────────────
    private readonly ClientReactive<string> _assessmentStep = new("purpose");    // purpose | details | result
    private readonly ClientReactive<string> _assessmentPurpose = new("");        // "work" | "study" | "family" | "other"
    private readonly ClientReactive<string> _assessmentWorkType = new("");       // "employed" | "self" | "au_pair" | etc.
    private readonly ClientReactive<string> _assessmentStudyLevel = new("");     // "higher" | "vocational" | "language" | etc.
    private readonly ClientReactive<string> _assessmentFamilyType = new("");     // "spouse" | "child" | "parent" | etc.
    private readonly ClientReactive<PermitCategory?> _assessmentResult = new(null);

    private void RenderPermitAssessment(UIView view)
    {
        view.Column(["min-h-full bg-gradient-to-br from-blue-50 to-blue-100 flex flex-col"], content: view =>
        {
            // Header
            view.Box(["bg-blue-600 text-white py-8 px-4"], content: view =>
            {
                view.Column([Container.Xl, "gap-2"], content: view =>
                {
                    view.Text([Text.H1, "text-white"], "Find Your Permit");
                    view.Text(["text-blue-100 text-lg"], "Answer a few questions to find the right residence permit for your situation");
                });
            });

            // Main content
            view.Column([Container.Xl, "flex-1 py-12 gap-8"], content: view =>
            {
                if (_assessmentStep.Value == "purpose")
                    RenderAssessmentPurpose(view);
                else if (_assessmentStep.Value == "details")
                    RenderAssessmentDetails(view);
                else
                    RenderAssessmentResult(view);

                // Skip/Cancel button
                view.Box(["mt-auto pt-8 border-t"], content: view =>
                {
                    view.Row(["gap-3 justify-center"], content: view =>
                    {
                        view.Button([Button.OutlineMd],
                            onClick: async () => { ResetAssessment(); Navigate("form"); },
                            content: v => v.Text([], "Skip Assessment"));
                    });
                });
            });
        });
    }

    private void RenderAssessmentPurpose(UIView view)
    {
        view.Column(["gap-6"], content: view =>
        {
            view.Text([Text.H2], "What brings you to Finland?");
            view.Text(["text-muted-foreground"], "Select the main purpose of your move");

            view.Row(["gap-4 flex-wrap"], content: view =>
            {
                AssessmentCard(view, "Work", "You've found a job or want to start a business", "briefcase", "work", "🇫🇮 Employee, Self-Employed, Specialist");
                AssessmentCard(view, "Study", "You're pursuing education at an institution", "graduation-cap", "study", "📚 University, Vocational, Language Courses");
                AssessmentCard(view, "Family", "You're joining family members in Finland", "heart", "family", "❤️ Spouse, Children, Parents");
                AssessmentCard(view, "Other", "Protection, Citizenship, or Special Cases", "shield", "other", "🛡️ Asylum, Remigration, Visiting");
            });
        });
    }

    private void RenderAssessmentDetails(UIView view)
    {
        view.Column(["gap-6"], content: view =>
        {
            var purpose = _assessmentPurpose.Value;

            if (purpose == "work")
            {
                view.Text([Text.H2], "What's your employment situation?");
                view.Row(["gap-4 flex-wrap"], content: view =>
                {
                    DetailCard(view, "Employee", "You have a job contract with a Finnish employer", "work_employee");
                    DetailCard(view, "Specialist", "You have expertise in a shortage field (IT, Engineering, etc.)", "work_specialist");
                    DetailCard(view, "Self-Employed", "You run your own business or are a freelancer", "work_self");
                    DetailCard(view, "Researcher", "You're conducting research at a Finnish institution", "work_researcher");
                    DetailCard(view, "Seasonal", "You're working seasonally (agriculture, tourism)", "work_seasonal");
                    DetailCard(view, "Au Pair", "You're an au pair caring for a family's children", "work_au_pair");
                    DetailCard(view, "Startup", "You're launching a tech startup", "work_startup");
                    DetailCard(view, "Other Work", "Another work-related situation", "work_other");
                });
            }
            else if (purpose == "study")
            {
                view.Text([Text.H2], "What are you studying?");
                view.Row(["gap-4 flex-wrap"], content: view =>
                {
                    DetailCard(view, "University Degree", "Bachelor's or Master's at a higher education institution", "study_higher");
                    DetailCard(view, "PhD/Doctoral", "Pursuing a PhD or doctoral degree", "study_phd");
                    DetailCard(view, "Vocational Training", "Vocational or professional education", "study_vocational");
                    DetailCard(view, "Language Course", "Short-term language or specialized course", "study_language");
                    DetailCard(view, "Exchange Program", "International student exchange (Erasmus, etc.)", "study_exchange");
                    DetailCard(view, "Research", "Post-doc or research fellowship", "study_research");
                });
            }
            else if (purpose == "family")
            {
                view.Text([Text.H2], "What's your family relationship?");
                view.Row(["gap-4 flex-wrap"], content: view =>
                {
                    DetailCard(view, "Spouse/Partner", "You're married/in civil union with a Finnish person", "family_spouse_finnish");
                    DetailCard(view, "Spouse of Non-Finn", "Your spouse is an EU citizen/permit holder in Finland", "family_spouse_other");
                    DetailCard(view, "Child", "You're joining your parents who are in Finland", "family_child");
                    DetailCard(view, "Parent", "You're joining your child who is in Finland", "family_parent");
                    DetailCard(view, "Other Family", "Another family member (sibling, grandparent, etc.)", "family_other");
                });
            }
            else
            {
                view.Text([Text.H2], "Tell us more");
                view.Row(["gap-4 flex-wrap"], content: view =>
                {
                    DetailCard(view, "International Protection", "Fleeing conflict or persecution", "other_asylum");
                    DetailCard(view, "Return to Finland", "You're Finnish or previously lived here", "other_return");
                    DetailCard(view, "Visitor/Scholar", "Visiting researcher, lecturer, or short-term visitor", "other_visitor");
                });
            }

            view.Button([Button.OutlineMd, "mt-4"],
                onClick: async () => { _assessmentStep.Value = "purpose"; },
                content: v => v.Text([], "← Back"));
        });
    }

    private void AssessmentCard(UIView view, string title, string desc, string icon, string purposeValue, string tags)
    {
        var isSelected = _assessmentPurpose.Value == purposeValue;
        view.Button(
            isSelected
                ? [Card.Default, "flex-1 min-w-[240px] p-5 border-2 border-primary bg-primary/5 cursor-pointer text-left"]
                : [Card.Default, "flex-1 min-w-[240px] p-5 cursor-pointer text-left hover:border-primary/50"],
            onClick: async () =>
            {
                _assessmentPurpose.Value = purposeValue;
                _assessmentStep.Value = "details";
            },
            content: v =>
            {
                v.Row(["items-start gap-3 mb-2"], content: view =>
                {
                    v.Icon([isSelected ? "text-primary" : "text-muted-foreground", "w-6 h-6"], name: icon);
                    v.Text(["font-semibold text-base"], title);
                });
                v.Text(["text-xs text-muted-foreground mb-2"], desc);
                v.Text(["text-[10px] text-muted-foreground"], tags);
            });
    }

    private void DetailCard(UIView view, string title, string desc, string detailValue)
    {
        view.Button(
            [Card.Default, "flex-1 min-w-[220px] p-4 cursor-pointer hover:border-primary/50 text-left transition"],
            onClick: async () =>
            {
                _assessmentWorkType.Value = detailValue.StartsWith("work_") ? detailValue : "";
                _assessmentStudyLevel.Value = detailValue.StartsWith("study_") ? detailValue : "";
                _assessmentFamilyType.Value = detailValue.StartsWith("family_") ? detailValue : "";
                _assessmentStep.Value = "result";
                ComputeAssessmentResult();
            },
            content: v =>
            {
                v.Text(["font-semibold text-sm"], title);
                v.Text(["text-xs text-muted-foreground mt-1"], desc);
            });
    }

    private void RenderAssessmentResult(UIView view)
    {
        if (_assessmentResult.Value == null)
        {
            view.Text(["text-center"], "Processing your answers...");
            return;
        }

        var result = _assessmentResult.Value.Value;
        var displayName = result.DisplayName();
        var shortName = result.ShortName();
        var icon = result.CategoryIcon();
        var url = result.MigriFiUrl();

        view.Column(["gap-6"], content: view =>
        {
            // Result card
            view.Box([Card.Default, "p-8 border-l-4 border-l-primary bg-primary/2 gap-4"], content: view =>
            {
                view.Row(["items-start gap-4 mb-4"], content: view =>
                {
                    view.Icon(["text-primary w-12 h-12"], name: icon);
                    view.Column(["gap-1"], content: view =>
                    {
                        view.Text([Text.H2, "text-primary"], displayName);
                        view.Text(["text-muted-foreground"], "This permit matches your situation");
                    });
                });

                view.Separator(["my-2"]);

                // Quick facts
                view.Text(["text-sm font-medium mb-2"], "Key Requirements:");
                var rule = PermitRules.For(result);
                if (rule.MinSalary.HasValue)
                    view.Row(["items-center gap-2 text-sm"], content: v =>
                    {
                        v.Icon(["w-4 h-4 text-green-600"], name: "check");
                        v.Text([], $"Minimum salary: €{rule.MinSalary:N0}/month");
                    });
                if (rule.MinFunds.HasValue)
                    view.Row(["items-center gap-2 text-sm"], content: v =>
                    {
                        v.Icon(["w-4 h-4 text-green-600"], name: "check");
                        v.Text([], $"Minimum funds: €{rule.MinFunds:N0}/month");
                    });

                view.Text(["text-xs text-muted-foreground mt-3"],
                    $"Learn more: {url}");
            });

            // Action buttons
            view.Row(["gap-3"], content: view =>
            {
                view.Button([Button.PrimaryMd, "flex-1"],
                    onClick: async () =>
                    {
                        _permitCategory.Value = result.ToString();
                        UpdatePermitGroup();
                        // If on applicant home, go straight to applying mode; otherwise navigate to form
                        if (_homePanel.Value != null && (_homePanel.Value == "assessment" || _homePanel.Value == "applying"))
                        {
                            ResetAssessment();
                            _homePanel.Value = "applying";
                        }
                        else
                        {
                            ResetAssessment();
                            Navigate("form");
                        }
                    },
                    content: v => v.Text([], $"Apply for {shortName}"));

                view.Button([Button.OutlineMd, "flex-1"],
                    onClick: async () => { _assessmentStep.Value = "details"; },
                    content: v => v.Text([], "← Try Another"));
            });
        });
    }

    private void ComputeAssessmentResult()
    {
        var purpose = _assessmentPurpose.Value;
        var workType = _assessmentWorkType.Value;
        var studyLevel = _assessmentStudyLevel.Value;
        var familyType = _assessmentFamilyType.Value;

        var result = (purpose, workType, studyLevel, familyType) switch
        {
            ("work", "work_employee", _, _) => PermitCategory.EmployeePermit,
            ("work", "work_specialist", _, _) => PermitCategory.SpecialistExpert,
            ("work", "work_self", _, _) => PermitCategory.SelfEmployed,
            ("work", "work_researcher", _, _) => PermitCategory.Researcher,
            ("work", "work_seasonal", _, _) => PermitCategory.SeasonalWorker,
            ("work", "work_au_pair", _, _) => PermitCategory.AuPair,
            ("work", "work_startup", _, _) => PermitCategory.StartupEntrepreneur,
            ("work", "work_other", _, _) => PermitCategory.EmployeePermit,

            ("study", _, "study_higher", _) => PermitCategory.StudentHigherEd,
            ("study", _, "study_phd", _) => PermitCategory.StudentPHD,
            ("study", _, "study_vocational", _) => PermitCategory.StudentVocational,
            ("study", _, "study_language", _) => PermitCategory.LanguageCourse,
            ("study", _, "study_exchange", _) => PermitCategory.ExchangeStudent,
            ("study", _, "study_research", _) => PermitCategory.PostDocResearcher,

            ("family", _, _, "family_spouse_finnish") => PermitCategory.SpouseOfFinnish,
            ("family", _, _, "family_spouse_other") => PermitCategory.SpouseOfPermitHolder,
            ("family", _, _, "family_child") => PermitCategory.ChildOfPermitHolder,
            ("family", _, _, "family_parent") => PermitCategory.ParentOfMinorPermitHolder,
            ("family", _, _, "family_other") => PermitCategory.DependentFamily,

            ("other", _, _, _) when workType.Contains("asylum") => PermitCategory.Asylum,
            ("other", _, _, _) when workType.Contains("return") => PermitCategory.Remigration,
            ("other", _, _, _) => PermitCategory.VisitingResearcher,

            _ => PermitCategory.StudentHigherEd
        };

        _assessmentResult.Value = result;
    }

    private void ResetAssessment()
    {
        _assessmentStep.Value = "purpose";
        _assessmentPurpose.Value = "";
        _assessmentWorkType.Value = "";
        _assessmentStudyLevel.Value = "";
        _assessmentFamilyType.Value = "";
        _assessmentResult.Value = null;
    }

    private void UpdatePermitGroup()
    {
        var cat = Enum.TryParse<PermitCategory>(_permitCategory.Value, out var c) ? c : PermitCategory.StudentHigherEd;
        var group = cat.GetGroup();
        _permitGroup.Value = group switch
        {
            PermitGroup.Work => "Work",
            PermitGroup.Study => "Study",
            PermitGroup.Family => "Family",
            _ => "Study"
        };
    }

    // ── Assessment inline version (for applicant home panel) ────────────────

    private void RenderPermitAssessmentInline(UIView view)
    {
        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column(["px-6 py-8 gap-6 min-h-full"], content: view =>
            {
                view.Text([Text.H2], "Find Your Permit");
                view.Text(["text-muted-foreground text-sm"], "Answer a few quick questions");

                if (_assessmentStep.Value == "purpose")
                    RenderAssessmentPurpose(view);
                else if (_assessmentStep.Value == "details")
                    RenderAssessmentDetails(view);
                else
                    RenderAssessmentResult(view);

                // Back button
                if (_assessmentStep.Value == "result")
                {
                    view.Button([Button.OutlineSm],
                        onClick: async () => { _assessmentStep.Value = "details"; },
                        content: v => v.Text([], "← Try Another"));
                }
            });
        });
    }
}
