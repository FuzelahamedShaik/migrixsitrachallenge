public partial class IkonDemoApp
{
    // ── Quiz State ────────────────────────────────────────────────────────────
    private readonly ClientReactive<string> _quizPhase = new("start");           // start | questions | results
    private readonly ClientReactive<int> _quizCurrentQuestion = new(0);
    private readonly ClientReactive<List<string>> _quizAnswers = new([]);
    private readonly ClientReactive<List<PermitCategory>> _quizResults = new([]);

    private void RenderPermitQuizInline(UIView view)
    {
        view.ScrollArea(rootStyle: ["h-full"], content: view =>
        {
            view.Column(["px-6 py-8 gap-6 min-h-full"], content: view =>
            {
                if (_quizPhase.Value == "start")
                    RenderQuizStart(view);
                else if (_quizPhase.Value == "questions")
                    RenderQuizQuestion(view);
                else
                    RenderQuizResults(view);
            });
        });
    }

    // ── PHASE 1: Start Screen ─────────────────────────────────────────────────

    private void RenderQuizStart(UIView view)
    {
        view.Column(["gap-8 max-w-2xl"], content: view =>
        {
            view.Column(["gap-3 py-4"], content: view =>
            {
                view.Text([Text.H1, "text-3xl font-bold"], "🇫🇮 Find Your Perfect Permit");
                view.Text(["text-base text-muted-foreground"], "Answer 12 quick questions to discover your ideal residence permit");
            });

            view.Box([Card.Default, "p-6 bg-blue-50 border-2 border-blue-200 gap-4"], content: view =>
            {
                view.Row(["items-start gap-3"], content: view =>
                {
                    view.Icon(["text-blue-600 w-6 h-6"], name: "lightbulb");
                    view.Column(["gap-2"], content: view =>
                    {
                        view.Text(["font-semibold"], "How it works");
                        view.Column(["gap-1 text-sm text-muted-foreground"], content: view =>
                        {
                            view.Text([], "• 12 targeted questions (2 min)");
                            view.Text([], "• Smart matching against 59 permits");
                            view.Text([], "• Top 3 recommendations ranked");
                            view.Text([], "• Start application instantly");
                        });
                    });
                });
            });

            view.Column(["gap-3 mt-4"], content: view =>
            {
                view.Text(["text-sm font-semibold uppercase text-muted-foreground"], "What's your main goal?");
                view.Row(["gap-3 flex-wrap"], content: view =>
                {
                    StartButton(view, "💼", "Work", "Employment, business, or career", "work");
                    StartButton(view, "📚", "Study", "Education, research, or training", "study");
                    StartButton(view, "❤️", "Family", "Join family in Finland", "family");
                    StartButton(view, "🛡️", "Protection", "Asylum or international protection", "protection");
                });
            });
        });
    }

    private void StartButton(UIView view, string emoji, string title, string desc, string category)
    {
        view.Button([Card.Default, "flex-1 min-w-[140px] p-4 text-left cursor-pointer hover:shadow-md hover:border-primary transition"],
            onClick: async () =>
            {
                _quizAnswers.Value = [category];  // First answer is category
                _quizCurrentQuestion.Value = 0;
                _quizPhase.Value = "questions";
            },
            content: v =>
            {
                v.Column(["gap-2"], content: view =>
                {
                    v.Text(["text-2xl"], emoji);
                    v.Text(["font-semibold"], title);
                    v.Text(["text-xs text-muted-foreground"], desc);
                });
            });
    }

    // ── PHASE 2: Questions ────────────────────────────────────────────────────

    private void RenderQuizQuestion(UIView view)
    {
        var category = _quizAnswers.Value.FirstOrDefault() ?? "work";
        var questions = GetQuizQuestions(category);
        var questionCount = questions.Count;

        // Safety check
        if (_quizCurrentQuestion.Value >= questionCount)
        {
            _quizPhase.Value = "results";
            ComputeQuizResults();
            return;
        }

        var q = questions[_quizCurrentQuestion.Value];
        var progress = ((_quizCurrentQuestion.Value + 1) / (decimal)questionCount) * 100;
        var isLastQuestion = _quizCurrentQuestion.Value == questionCount - 1;

        view.Column(["gap-6"], content: view =>
        {
            // Progress
            view.Box([Progress.Root], content: v =>
                v.Box([Progress.Indicator, Progress.IndicatorTransform((int)progress)]));
            view.Row(["justify-between text-xs text-muted-foreground"], content: v =>
            {
                v.Text([], $"Question {_quizCurrentQuestion.Value + 1}/{questionCount}");
                v.Text([], $"{(int)progress}%");
            });

            // Question
            view.Column(["gap-3 mt-4"], content: view =>
            {
                view.Text([Text.H2, "text-xl font-semibold"], q.Title);
                if (!string.IsNullOrEmpty(q.Description))
                    view.Text(["text-sm text-muted-foreground"], q.Description);

                // Answers
                view.Column(["gap-2 mt-4"], content: view =>
                {
                    foreach (var ans in q.Answers)
                    {
                        var answer = ans;
                        var answerIndex = _quizCurrentQuestion.Value;
                        var isSelected = _quizAnswers.Value.Count > answerIndex && _quizAnswers.Value[answerIndex] == answer.Value;

                        view.Button([Card.Default,
                            isSelected ? "border-2 border-primary bg-primary/5" : "hover:border-primary/50",
                            "w-full p-4 text-left cursor-pointer transition"],
                            onClick: async () =>
                            {
                                // Update or add answer at this question index
                                var answers = new List<string>(_quizAnswers.Value);
                                // Pad with empty strings if needed
                                while (answers.Count <= answerIndex)
                                    answers.Add("");
                                answers[answerIndex] = answer.Value;
                                _quizAnswers.Value = answers;
                            },
                            content: v =>
                            {
                                v.Row(["items-start gap-3"], content: view =>
                                {
                                    v.Icon(["w-5 h-5", isSelected ? "text-primary" : "text-muted-foreground"],
                                        name: isSelected ? "check-circle-2" : "circle");
                                    v.Column(["flex-1 gap-0.5"], content: view =>
                                    {
                                        v.Text(["font-medium text-sm"], answer.Label);
                                        if (!string.IsNullOrEmpty(answer.Description))
                                            v.Text(["text-xs text-muted-foreground"], answer.Description);
                                    });
                                });
                            });
                    }
                });
            });

            // Navigation
            view.Row(["gap-3 justify-between mt-6"], content: view =>
            {
                view.Button([Button.OutlineSm],
                    disabled: _quizCurrentQuestion.Value == 0,
                    onClick: async () => { _quizCurrentQuestion.Value--; },
                    content: v => v.Text([], "← Back"));

                if (isLastQuestion)
                {
                    view.Button([Button.PrimarySm, "gap-2"],
                        onClick: async () =>
                        {
                            ComputeQuizResults();
                            _quizPhase.Value = "results";
                        },
                        content: v =>
                        {
                            v.Icon(["w-4 h-4"], name: "check");
                            v.Text([], "See Results");
                        });
                }
                else
                {
                    view.Button([Button.PrimarySm],
                        onClick: async () => { _quizCurrentQuestion.Value++; },
                        content: v => v.Text([], "Next →"));
                }
            });
        });
    }

    // ── PHASE 3: Results ──────────────────────────────────────────────────────

    private void RenderQuizResults(UIView view)
    {
        if (_quizResults.Value.Count == 0)
            _quizResults.Value = [PermitCategory.StudentHigherEd];

        var topResult = _quizResults.Value.First();
        var topRule = PermitRules.For(topResult);

        view.Column(["gap-6"], content: view =>
        {
            view.Column(["gap-2 text-center py-4"], content: view =>
            {
                view.Text([Text.H1, "text-2xl"], "✅ Your Best Match");
                view.Text(["text-muted-foreground"], $"Based on your answers ({_quizAnswers.Value.Count} questions)");
            });

            // Top result
            RenderResultCard(view, topResult, topRule, true);

            // Alternatives
            if (_quizResults.Value.Count > 1)
            {
                view.Column(["gap-2 mt-6"], content: view =>
                {
                    view.Text(["text-xs font-semibold text-muted-foreground uppercase"], "Other options:");
                    foreach (var alt in _quizResults.Value.Skip(1).Take(2))
                    {
                        var altRule = PermitRules.For(alt);
                        RenderResultCard(view, alt, altRule, false);
                    }
                });
            }

            // CTAs
            view.Row(["gap-2 mt-6"], content: view =>
            {
                view.Button([Button.PrimaryMd, "flex-1 gap-2"],
                    onClick: async () =>
                    {
                        _permitCategory.Value = topResult.ToString();
                        var group = topResult.GetGroup();
                        _permitGroup.Value = group switch
                        {
                            PermitGroup.Work => "Work",
                            PermitGroup.Study => "Study",
                            PermitGroup.Family => "Family",
                            PermitGroup.Protection => "Protection",
                            _ => "Study"
                        };
                        ResetQuiz();
                        _homePanel.Value = "applying";
                    },
                    content: v =>
                    {
                        v.Icon(["w-4 h-4"], name: "arrow-right");
                        v.Text([], $"Apply for {topResult.ShortName()}");
                    });

                view.Button([Button.OutlineSm, "flex-1"],
                    onClick: async () =>
                    {
                        ResetQuiz();
                        _quizPhase.Value = "start";
                    },
                    content: v => v.Text([], "Restart Quiz"));
            });
        });
    }

    private void RenderResultCard(UIView view, PermitCategory result, PermitRule rule, bool isTop)
    {
        var displayName = result.DisplayName();
        var icon = result.CategoryIcon();
        var url = result.MigriFiUrl();

        var bg = isTop ? "bg-primary/5" : "bg-muted/20";
        var border = isTop ? "border-l-4 border-l-primary" : "";

        view.Box([Card.Default, $"p-5 {bg} {border} gap-4"], content: view =>
        {
            view.Row(["items-start gap-4"], content: view =>
            {
                view.Box(["w-12 h-12 rounded-lg bg-background border border-border flex items-center justify-center"],
                    content: v => v.Icon(["w-6 h-6 text-primary"], name: icon));

                view.Column(["flex-1 gap-1"], content: view =>
                {
                    view.Row(["items-center gap-2"], content: view =>
                    {
                        view.Text(["font-semibold"], displayName);
                        if (isTop)
                            view.Box(["bg-primary text-white text-[10px] font-bold px-2 py-0.5 rounded"],
                                content: v => v.Text([], "BEST MATCH"));
                    });

                    view.Column(["gap-1 text-xs text-muted-foreground mt-2"], content: view =>
                    {
                        if (rule.MinSalary.HasValue)
                            view.Text([], $"💰 Min: €{rule.MinSalary:N0}/month");
                        if (rule.MinFunds.HasValue)
                            view.Text([], $"💵 Min funds: €{rule.MinFunds:N0}/month");
                    });
                });
            });
        });
    }

    // ── Quiz Data & Scoring ───────────────────────────────────────────────────

    private record QuizQuestion(string Title, string Description, List<QuizAnswer> Answers);
    private record QuizAnswer(string Label, string Description, string Value);

    private List<QuizQuestion> GetQuizQuestions(string category) => category switch
    {
        "work" => GetWorkQuestions(),
        "study" => GetStudyQuestions(),
        "family" => GetFamilyQuestions(),
        "protection" => GetProtectionQuestions(),
        _ => []
    };

    private List<QuizQuestion> GetWorkQuestions()
    {
        return new()
        {
            new("What's your employment situation?", "",
                new() {
                    new("I have a confirmed job offer", "", "job_offer"),
                    new("I'm a highly skilled specialist/expert", "IT, Engineering, Finance", "specialist"),
                    new("I want to start my own business", "Entrepreneur/startup", "entrepreneur"),
                    new("I'm doing research or academia", "University, research institute", "researcher"),
                    new("I'm a seasonal/temporary worker", "Agriculture, tourism, construction", "seasonal"),
                    new("I'm an au pair or nanny", "Childcare provider", "au_pair"),
                }),

            new("What will your monthly gross income be?", "Approximately, before tax",
                new() {
                    new("€1,200 - €2,000", "Entry level", "salary_low"),
                    new("€2,000 - €3,500", "Mid-level", "salary_mid"),
                    new("€3,500 - €6,000", "Senior level", "salary_high"),
                    new("€6,000+", "Executive/specialist", "salary_very_high"),
                }),

            new("What's your highest education level?", "",
                new() {
                    new("High school/secondary", "", "ed_hs"),
                    new("Bachelor's degree", "", "ed_bachelor"),
                    new("Master's degree", "", "ed_master"),
                    new("PhD or doctorate", "", "ed_phd"),
                }),

            new("How many years of work experience do you have?", "In your field",
                new() {
                    new("Less than 1 year", "Fresh graduate", "exp_0"),
                    new("1-3 years", "", "exp_1_3"),
                    new("3-5 years", "", "exp_3_5"),
                    new("5+ years", "", "exp_5plus"),
                }),

            new("Will you have an employment contract before arriving?", "",
                new() {
                    new("Yes, signed contract ready", "", "contract_yes"),
                    new("Yes, job offer letter", "", "offer_yes"),
                    new("No, hiring after arrival", "", "contract_no"),
                    new("Not applicable, self-employed", "", "contract_na"),
                }),

            new("Does your job match a shortage occupation?", "High-demand sectors: IT, healthcare, construction, engineering",
                new() {
                    new("Yes, it's in high demand", "", "shortage_yes"),
                    new("No, general occupation", "", "shortage_no"),
                    new("Not sure", "", "shortage_unsure"),
                }),

            new("Will you be transferring within your company?", "Same employer, different country",
                new() {
                    new("Yes, intra-corporate transfer", "", "ict_yes"),
                    new("No, different employer", "", "ict_no"),
                    new("Not applicable", "", "ict_na"),
                }),

            new("Do you have the required documents?", "",
                new() {
                    new("Yes, all ready", "Contract, passport, proof of funds", "docs_ready"),
                    new("Most documents ready", "", "docs_partial"),
                    new("Haven't started gathering yet", "", "docs_none"),
                }),

            new("What type of work visa interests you?", "",
                new() {
                    new("Standard employee permit", "Regular employment", "type_employee"),
                    new("EU Blue Card", "Highly skilled worker", "type_blue_card"),
                    new("Self-employed/entrepreneur", "Own business", "type_self"),
                    new("Working holiday", "Young professional (age limit apply)", "type_holiday"),
                }),

            new("How long do you plan to work in Finland?", "",
                new() {
                    new("1-2 years", "Short-term assignment", "duration_short"),
                    new("2-4 years", "Medium-term", "duration_med"),
                    new("4+ years", "Long-term/permanent", "duration_long"),
                    new("Unsure", "", "duration_unsure"),
                }),

            new("Do you speak Finnish or Swedish?", "",
                new() {
                    new("Fluent in Finnish", "", "lang_finnish"),
                    new("Basic Finnish", "", "lang_basic_fi"),
                    new("Some Swedish", "", "lang_swedish"),
                    new("English only", "", "lang_english"),
                }),

            new("Will you need visa support from your employer?", "",
                new() {
                    new("Yes, employer will sponsor", "", "sponsor_yes"),
                    new("No, I'll handle it myself", "", "sponsor_no"),
                    new("Not sure", "", "sponsor_unsure"),
                }),
        };
    }

    private List<QuizQuestion> GetStudyQuestions()
    {
        return new()
        {
            new("What level of education?", "",
                new() {
                    new("University (Bachelor/Master)", "", "study_uni"),
                    new("PhD or doctoral degree", "Research-focused", "study_phd"),
                    new("Vocational or professional", "Trades, nursing, etc.", "study_vocational"),
                    new("Short course or language school", "< 1 year program", "study_short"),
                    new("Exchange semester", "Erasmus+ or similar", "study_exchange"),
                    new("Research fellowship", "Post-doc or visiting scholar", "study_research"),
                }),

            new("How long will your program last?", "",
                new() {
                    new("Less than 1 year", "", "duration_under_1y"),
                    new("1-2 years", "", "duration_1_2y"),
                    new("2-4 years", "", "duration_2_4y"),
                    new("4+ years (PhD)", "", "duration_4plus"),
                }),

            new("Do you have an acceptance letter?", "",
                new() {
                    new("Yes, confirmed and accepted", "", "admission_confirmed"),
                    new("Yes, conditional acceptance", "", "admission_conditional"),
                    new("Not yet, still applying", "", "admission_pending"),
                    new("Already studying, extending", "", "admission_extend"),
                }),

            new("How will you fund your studies?", "",
                new() {
                    new("Scholarship/grant (full/partial)", "", "fund_scholarship"),
                    new("Own savings or family support", "", "fund_own"),
                    new("Part-time work while studying", "", "fund_work"),
                    new("Combination of sources", "", "fund_mixed"),
                }),

            new("What's your monthly budget?", "Total available funds per month",
                new() {
                    new("€500-800", "", "budget_low"),
                    new("€800-1,200", "", "budget_mid"),
                    new("€1,200-1,800", "", "budget_high"),
                    new("€1,800+", "", "budget_very_high"),
                }),

            new("Previous degree field?", "",
                new() {
                    new("STEM (Science/Tech/Engineering/Math)", "", "field_stem"),
                    new("Humanities or social sciences", "", "field_humanities"),
                    new("Business or economics", "", "field_business"),
                    new("Other or no degree yet", "", "field_other"),
                }),

            new("English proficiency level?", "For studying in English-taught programs",
                new() {
                    new("Native speaker", "", "english_native"),
                    new("Fluent/Advanced", "", "english_fluent"),
                    new("Upper-intermediate", "", "english_intermediate"),
                    new("Beginner", "", "english_beginner"),
                }),

            new("Do you have academic transcripts ready?", "",
                new() {
                    new("Yes, official transcripts ready", "", "transcripts_ready"),
                    new("Yes, but need translation", "", "transcripts_translate"),
                    new("Not available yet", "", "transcripts_pending"),
                }),

            new("Will you need accommodation help?", "",
                new() {
                    new("Yes, university housing needed", "", "accom_uni"),
                    new("Yes, help finding private", "", "accom_private"),
                    new("No, already arranged", "", "accom_arranged"),
                    new("Haven't thought about it", "", "accom_none"),
                }),

            new("Are you applying to multiple institutions?", "",
                new() {
                    new("One university", "", "app_one"),
                    new("2-3 universities", "", "app_few"),
                    new("4+ universities (safety net)", "", "app_many"),
                    new("Direct transfer/already admitted", "", "app_direct"),
                }),

            new("Do you plan to work after graduation?", "",
                new() {
                    new("Yes, stay and work in Finland", "", "post_work_yes"),
                    new("Maybe, depends on opportunities", "", "post_work_maybe"),
                    new("No, return home", "", "post_work_no"),
                    new("Undecided", "", "post_work_undecided"),
                }),

            new("Have you researched Finnish universities?", "",
                new() {
                    new("Yes, thoroughly", "Top 3 choices identified", "research_thorough"),
                    new("Somewhat familiar", "", "research_some"),
                    new("Just starting", "", "research_begin"),
                    new("No, need recommendations", "", "research_none"),
                }),

            new("What's your study start timeline?", "",
                new() {
                    new("This autumn (Sep/Aug)", "", "timeline_soon"),
                    new("Next spring (Jan/Feb)", "", "timeline_spring"),
                    new("Next year", "", "timeline_next_year"),
                    new("Flexible/not urgent", "", "timeline_flexible"),
                }),
        };
    }

    private List<QuizQuestion> GetFamilyQuestions()
    {
        return new()
        {
            new("What's your family relationship?", "",
                new() {
                    new("Spouse/civil partner", "", "rel_spouse"),
                    new("Child of parent in Finland", "Dependency", "rel_child"),
                    new("Parent of child in Finland", "Your child is there", "rel_parent"),
                    new("Other family member", "Sibling, grandparent, etc.", "rel_other"),
                }),

            new("Who is your sponsor in Finland?", "",
                new() {
                    new("Finnish citizen", "", "sponsor_finnish"),
                    new("Holder of residence permit", "", "sponsor_permit_holder"),
                    new("EU citizen", "", "sponsor_eu"),
                    new("Registered partnership", "Not legal marriage", "sponsor_registered"),
                }),

            new("Is your relationship documented?", "",
                new() {
                    new("Yes, legal marriage/partnership", "", "doc_marriage"),
                    new("Yes, marriage certificate ready", "", "doc_cert_ready"),
                    new("Cohabitation, not married", "Registered or not", "doc_cohabitation"),
                    new("Engaged, not married yet", "", "doc_engaged"),
                }),

            new("How long have you been together?", "",
                new() {
                    new("< 6 months", "", "duration_new"),
                    new("6 months - 1 year", "", "duration_new_mid"),
                    new("1-2 years", "", "duration_1_2y"),
                    new("2+ years", "Established relationship", "duration_long"),
                }),

            new("Do you have children together?", "",
                new() {
                    new("Yes, joint children", "", "children_yes"),
                    new("No, but planning to", "", "children_no_plan"),
                    new("No, not planning", "", "children_no"),
                    new("I have children from previous", "", "children_previous"),
                }),

            new("Has your sponsor lived in Finland long?", "",
                new() {
                    new("Permanent resident (5+ years)", "", "sponsor_perm"),
                    new("Continuous resident (2-5 years)", "", "sponsor_continuous"),
                    new("Recently moved (< 2 years)", "", "sponsor_recent"),
                    new("Just relocating/not sure", "", "sponsor_unsure"),
                }),

            new("What's your employment plan?", "",
                new() {
                    new("I'll work full-time", "", "employ_full"),
                    new("I'll work part-time", "", "employ_part"),
                    new("I'll be a homemaker", "", "employ_none"),
                    new("Not sure yet", "", "employ_unsure"),
                }),

            new("Language capability?", "",
                new() {
                    new("Fluent in Finnish", "", "lang_finnish"),
                    new("Basic Finnish", "", "lang_basic"),
                    new("English only", "", "lang_english"),
                    new("Another Scandinavian language", "", "lang_other"),
                }),

            new("Financial situation?", "",
                new() {
                    new("Sponsor has stable income", "", "finance_good"),
                    new("Sponsor works but modest income", "", "finance_modest"),
                    new("Both will work to support", "", "finance_both"),
                    new("Uncertain/still assessing", "", "finance_unsure"),
                }),

            new("Housing arranged in Finland?", "",
                new() {
                    new("Yes, house/apartment ready", "", "housing_ready"),
                    new("Sponsor has housing", "", "housing_sponsor"),
                    new("Need to find accommodation", "", "housing_search"),
                    new("Planning to buy/rent together", "", "housing_plan"),
                }),

            new("What's your main concern?", "",
                new() {
                    new("Language barrier", "", "concern_language"),
                    new("Job prospects", "", "concern_job"),
                    new("Integration/social", "", "concern_integration"),
                    new("None, very confident", "", "concern_none"),
                }),

            new("When do you want to move?", "",
                new() {
                    new("ASAP (next 1-3 months)", "", "timeline_asap"),
                    new("This year", "", "timeline_year"),
                    new("Next year", "", "timeline_next"),
                    new("Flexible/planning ahead", "", "timeline_flexible"),
                }),
        };
    }

    private List<QuizQuestion> GetProtectionQuestions()
    {
        return new()
        {
            new("What type of protection do you need?", "",
                new() {
                    new("International protection/asylum", "Fleeing persecution", "type_asylum"),
                    new("Temporary protection", "War, conflict (Ukraine, Syria, etc.)", "type_temp"),
                    new("Victim of human trafficking", "Human rights protection", "type_trafficking"),
                    new("Returning to Finland", "Former resident/citizen", "type_returning"),
                    new("Apply for citizenship", "Want to become Finnish", "type_citizenship"),
                }),

            new("What country are you from?", "",
                new() {
                    new("EU/EEA country", "", "origin_eu"),
                    new("Middle East or North Africa", "", "origin_mena"),
                    new("Sub-Saharan Africa", "", "origin_ssa"),
                    new("Asia or other region", "", "origin_other"),
                }),

            new("Do you have a passport or ID?", "",
                new() {
                    new("Yes, valid passport", "", "doc_passport"),
                    new("Yes, but expired", "", "doc_expired"),
                    new("No, documents lost/unavailable", "", "doc_none"),
                    new("Partial documents", "", "doc_partial"),
                }),

            new("When did you last live in Finland?", "If applicable",
                new() {
                    new("Never lived there", "", "prior_never"),
                    new("< 5 years ago", "", "prior_recent"),
                    new("5-10 years ago", "", "prior_5_10y"),
                    new("10+ years ago", "", "prior_long"),
                }),

            new("Do you have family in Finland?", "",
                new() {
                    new("Yes, close family", "Parent, sibling, child", "family_yes"),
                    new("Yes, distant relatives", "", "family_distant"),
                    new("No family", "", "family_none"),
                    new("Unknown/uncertain", "", "family_unknown"),
                }),

            new("Employment prospects concern?", "",
                new() {
                    new("Major concern", "Very worried", "job_concern_high"),
                    new("Some concern", "", "job_concern_mid"),
                    new("Not worried", "", "job_concern_low"),
                    new("Haven't thought about it", "", "job_concern_none"),
                }),

            new("How soon do you need status?", "",
                new() {
                    new("Urgent, within weeks", "", "urgency_high"),
                    new("Within months", "", "urgency_med"),
                    new("Not urgent", "", "urgency_low"),
                    new("Uncertain", "", "urgency_unsure"),
                }),

            new("Do you have legal representation?", "",
                new() {
                    new("Yes, lawyer appointed", "", "legal_yes"),
                    new("Seeking legal help", "", "legal_seeking"),
                    new("No representation yet", "", "legal_no"),
                    new("Don't know if needed", "", "legal_unsure"),
                }),

            new("Primary goal in Finland?", "",
                new() {
                    new("Safety/stability", "", "goal_safety"),
                    new("Economic opportunity", "", "goal_work"),
                    new("Education", "", "goal_study"),
                    new("Family reunification", "", "goal_family"),
                }),

            new("Support system in place?", "",
                new() {
                    new("NGO or community support", "", "support_ngo"),
                    new("Government assistance", "", "support_gov"),
                    new("Family/friends support", "", "support_family"),
                    new("Limited/none yet", "", "support_none"),
                }),

            new("Health insurance status?", "",
                new() {
                    new("Valid insurance", "", "health_yes"),
                    new("Need to arrange", "", "health_need"),
                    new("Don't have", "", "health_no"),
                    new("Not sure if needed", "", "health_unsure"),
                }),

            new("When can you start the process?", "",
                new() {
                    new("Immediately", "", "start_now"),
                    new("Within 1 month", "", "start_soon"),
                    new("Within 3 months", "", "start_3m"),
                    new("Later/uncertain", "", "start_later"),
                }),
        };
    }

    private void ComputeQuizResults()
    {
        var answers = _quizAnswers.Value;
        var category = answers.FirstOrDefault() ?? "work";

        var results = category switch
        {
            "work" => ComputeWorkMatches(answers),
            "study" => ComputeStudyMatches(answers),
            "family" => ComputeFamilyMatches(answers),
            "protection" => ComputeProtectionMatches(answers),
            _ => [PermitCategory.StudentHigherEd]
        };

        _quizResults.Value = results;
    }

    private List<PermitCategory> ComputeWorkMatches(List<string> answers)
    {
        var scores = new Dictionary<PermitCategory, int>();

        if (answers.Contains("job_offer")) { scores[PermitCategory.EmployeePermit] = 100; scores[PermitCategory.SpecialistExpert] = 80; }
        if (answers.Contains("specialist")) { scores[PermitCategory.SpecialistExpert] = 100; scores[PermitCategory.EUBlueCard] = 95; }
        if (answers.Contains("entrepreneur")) { scores[PermitCategory.SelfEmployed] = 100; scores[PermitCategory.StartupEntrepreneur] = 95; }
        if (answers.Contains("researcher")) { scores[PermitCategory.Researcher] = 100; scores[PermitCategory.PostDocResearcher] = 90; }
        if (answers.Contains("seasonal")) { scores[PermitCategory.SeasonalWorker] = 100; }
        if (answers.Contains("au_pair")) { scores[PermitCategory.AuPair] = 100; }
        if (answers.Contains("salary_very_high")) { scores[PermitCategory.EUBlueCard] = (scores.ContainsKey(PermitCategory.EUBlueCard) ? scores[PermitCategory.EUBlueCard] : 0) + 10; }
        if (answers.Contains("ict_yes")) { scores[PermitCategory.IntraCorporate] = 100; }
        if (answers.Contains("type_blue_card")) { scores[PermitCategory.EUBlueCard] = 100; }

        return scores.OrderByDescending(x => x.Value).Select(x => x.Key).Take(3).ToList();
    }

    private List<PermitCategory> ComputeStudyMatches(List<string> answers)
    {
        var scores = new Dictionary<PermitCategory, int>();

        if (answers.Contains("study_uni")) { scores[PermitCategory.StudentHigherEd] = 100; }
        if (answers.Contains("study_phd")) { scores[PermitCategory.StudentPHD] = 100; }
        if (answers.Contains("study_vocational")) { scores[PermitCategory.StudentVocational] = 100; }
        if (answers.Contains("study_short")) { scores[PermitCategory.LanguageCourse] = 100; }
        if (answers.Contains("study_exchange")) { scores[PermitCategory.ExchangeStudent] = 100; }
        if (answers.Contains("study_research")) { scores[PermitCategory.PostDocResearcher] = 100; }

        return scores.OrderByDescending(x => x.Value).Select(x => x.Key).Take(3).ToList();
    }

    private List<PermitCategory> ComputeFamilyMatches(List<string> answers)
    {
        var scores = new Dictionary<PermitCategory, int>();

        if (answers.Contains("rel_spouse") && answers.Contains("sponsor_finnish")) { scores[PermitCategory.SpouseOfFinnish] = 100; }
        if (answers.Contains("rel_spouse") && answers.Contains("sponsor_permit_holder")) { scores[PermitCategory.SpouseOfPermitHolder] = 100; }
        if (answers.Contains("rel_spouse") && answers.Contains("sponsor_eu")) { scores[PermitCategory.SpouseOfEUCitizen] = 100; }
        if (answers.Contains("rel_child")) { scores[PermitCategory.ChildOfFinnish] = 100; scores[PermitCategory.ChildOfPermitHolder] = 95; }
        if (answers.Contains("rel_parent")) { scores[PermitCategory.ParentOfMinorPermitHolder] = 100; }
        if (answers.Contains("rel_other")) { scores[PermitCategory.DependentFamily] = 100; }

        return scores.OrderByDescending(x => x.Value).Select(x => x.Key).Take(3).ToList();
    }

    private List<PermitCategory> ComputeProtectionMatches(List<string> answers)
    {
        var scores = new Dictionary<PermitCategory, int>();

        if (answers.Contains("type_asylum")) { scores[PermitCategory.Asylum] = 100; }
        if (answers.Contains("type_temp")) { scores[PermitCategory.TemporaryProtection] = 100; }
        if (answers.Contains("type_trafficking")) { scores[PermitCategory.HumanTraffickingVictim] = 100; }
        if (answers.Contains("type_returning")) { scores[PermitCategory.Remigration] = 100; }
        if (answers.Contains("type_citizenship")) { scores[PermitCategory.Citizenship] = 100; }

        return scores.OrderByDescending(x => x.Value).Select(x => x.Key).Take(3).ToList();
    }

    private void ResetQuiz()
    {
        _quizPhase.Value = "start";
        _quizCurrentQuestion.Value = 0;
        _quizAnswers.Value = [];
        _quizResults.Value = [];
    }
}
