namespace PermitReady;

// ── Document upload ───────────────────────────────────────────────────────

public enum DocStatus { Idle, Uploading, Verifying, Verified, Warning, Failed }

public record UploadedDoc(
    string FileName,
    long   SizeBytes,
    DocStatus Status,
    List<string> Alerts,
    Dictionary<string, string> Extracted
)
{
    public string SizeLabel => SizeBytes < 1024 ? $"{SizeBytes} B"
                             : SizeBytes < 1024 * 1024 ? $"{SizeBytes / 1024} KB"
                             : $"{SizeBytes / (1024 * 1024)} MB";
}

public record PassportAnalysis(bool IsPassport, string HolderName, string ExpiryDate, string Nationality, string Notes);
public record BankStatementAnalysis(bool IsBankStatement, decimal AverageMonthlyBalance, string Currency, string Notes);
public record EmploymentContractAnalysis(
    bool IsContract,
    string EmployerName,      // extracted employer name from document
    string JobTitle,          // extracted job title / position
    decimal MonthlySalary,    // extracted gross monthly salary (0 if not found)
    string ContractRef,       // contract/agreement reference number
    string StartDate,         // employment start date (YYYY-MM-DD or empty)
    string Notes
);
public record SalaryProofAnalysis(
    bool IsSalaryProof,
    string EmployerName,      // employer shown on payslip / salary certificate
    decimal MonthlyAmount,    // gross monthly figure shown (0 if not found)
    string Period,            // pay period (e.g. "2024-12" or "December 2024")
    string Notes
);
public record GenericDocAnalysis(bool IsRelevant, string DocumentType, string Notes);

// ── Permit category system ────────────────────────────────────────────────

public enum PermitGroup { Work, Study, Family, Protection, Other }

public enum PermitType { Student, Work }  // kept for backward compat

public enum PermitCategory
{
    // ─────────────────────────────────────────────────────────────────────────
    // WORK-BASED PERMITS (30 categories)
    // ─────────────────────────────────────────────────────────────────────────

    // Employee & Entrepreneur (1-4)
    EmployeePermit,              // Employee's residence permit
    SpecialistExpert,            // Specialist / expert permit
    EUBlueCard,                  // EU Blue Card (high-skilled worker)
    StartupEntrepreneur,         // Start-up entrepreneur

    // Self-Employment & Management (5-7)
    SelfEmployed,                // Self-employed / entrepreneur
    IntraCorporate,              // Intra-corporate transferee (ICT)
    TopMiddleManagement,         // Top and middle management

    // Specialized Work (8-14)
    Researcher,                  // Researcher / research permit
    CulturalArtsWork,            // Cultural or arts work
    MassMediaWork,               // Mass media work
    ReligiousCommunityWorker,    // Religious community employee
    Athlete,                     // Athlete
    Coach,                       // Coach or trainer
    InternationalOrgWork,        // International organization work

    // Temporary & Special Work (15-20)
    SeasonalWorker,              // Seasonal worker
    AuPair,                      // Au pair
    WorkingHoliday,              // Working holiday permit
    InternshipProgram,           // Internship (exchange programme)
    VolunteerWork,               // Volunteer work
    CompanyPreparation,          // Company preparation and supervision work

    // Post-Study & Migration (21-25)
    LookForWork,                 // Look for work (post-graduation)
    DegreeCompletedInFinland,    // Degree-completed-in-Finland work permit
    ResearchCompletedInFinland,  // Research-completed-in-Finland work permit
    Remigration,                 // Remigration permit (returning Finns)
    MachineryDelivery,           // Machine/system delivery

    // Specialized Sectors (26-30)
    LanguageTeacher,             // Language teacher
    HealthcareWorker,            // Healthcare worker
    TechSpecialist,              // Tech specialist
    AgricultureWorker,           // Agricultural worker
    ConstructionWorker,          // Construction worker

    // ─────────────────────────────────────────────────────────────────────────
    // STUDY-BASED PERMITS (10 categories)
    // ─────────────────────────────────────────────────────────────────────────

    StudentHigherEd,             // Student — higher education (university)
    StudentVocational,           // Student — vocational education
    StudentPHD,                  // PhD student / doctoral student
    ExchangeStudent,             // Exchange student
    LanguageCourse,              // Language course / short-term study
    TraineeIntern,               // Trainee / intern (non-degree)
    PostDocResearcher,           // Post-doctoral researcher
    UniversityResearch,          // University research programme
    TrainingProgram,             // Training / preparation programme
    ApprenticeSummer,            // Apprentice / summer student

    // ─────────────────────────────────────────────────────────────────────────
    // FAMILY-BASED PERMITS (10 categories)
    // ─────────────────────────────────────────────────────────────────────────

    SpouseOfFinnish,             // Spouse / registered partner of Finnish citizen
    SpouseOfEUCitizen,           // Spouse / partner of EU citizen
    SpouseOfPermitHolder,        // Spouse / partner of residence permit holder
    ChildOfFinnish,              // Child of Finnish citizen
    ChildOfPermitHolder,         // Child of residence permit holder
    ChildOfEUCitizen,            // Child of EU citizen (family reunification)
    ParentOfMinorFinnish,        // Parent of minor Finnish citizen
    ParentOfMinorPermitHolder,   // Parent of minor residence permit holder
    OtherFamilyEU,               // Other family member (EU regulations)
    DependentFamily,             // Dependent family member

    // ─────────────────────────────────────────────────────────────────────────
    // PROTECTION-BASED PERMITS (4 categories)
    // ─────────────────────────────────────────────────────────────────────────

    Asylum,                      // Asylum / international protection
    TemporaryProtection,         // Temporary protection (e.g. Ukraine)
    HumanTraffickingVictim,      // Victim of human trafficking
    CompassionateGrounds,        // Residence on compassionate grounds

    // ─────────────────────────────────────────────────────────────────────────
    // OTHER PERMITS (5 categories)
    // ─────────────────────────────────────────────────────────────────────────

    PermanentResidence,          // Permanent residence permit
    EUCitizenRegistration,       // EU citizen registration / residence certificate
    ReturningResident,           // Returning resident (D visa)
    Citizenship,                 // Finnish citizenship
    VisitingResearcher           // Visiting researcher

    // Total: 30 + 10 + 10 + 4 + 5 = 59 profiles
}

// ── Enums ─────────────────────────────────────────────────────────────────

public enum RoutingType
{
    FastTrack,
    SupplementRequested,
    SpecialistReview
}

// ── Application Input ─────────────────────────────────────────────────────

public record ApplicationInput(
    string FullName,
    string Nationality,
    string Email,
    PermitCategory Category,

    // Study fields
    string? UniversityName,
    string? ProgramName,
    decimal? FundsAmount,

    // Work fields
    string? EmployerName,
    string? JobTitle,
    decimal? SalaryAmount,
    string? EmploymentContractRef,

    // Family fields
    string? SponsorName,
    string? SponsorPermitNumber,

    List<string> UploadedDocuments,
    DateTime? PassportExpiry,

    // AI-extracted fields from uploaded documents (keyed by "passport_holdername",
    // "passport_nationality", "bank_avgbalance", "bank_currency", etc.)
    // Null for legacy records — treated as unverified.
    Dictionary<string, string>? ExtractedDocFields = null
)
{
    public PermitGroup Group     => Category.GetGroup();
    public PermitType PermitType => Group == PermitGroup.Study ? PermitType.Student : PermitType.Work;
}

// ── Screening Result ──────────────────────────────────────────────────────

public record ScreeningResult(
    int CompletenessScore,
    int RiskScore,
    RoutingType Routing,
    List<string> MissingItems,
    List<string> RiskFlags
);

// ── Payment ───────────────────────────────────────────────────────────────

public enum PaymentStatus
{
    Pending,   // Fee not yet paid — Migri cannot start processing
    Paid,      // Online payment confirmed
    Waived,    // Fee waived (e.g. EU/EEA, asylum — not chargeable)
}

// ── Stored Application ────────────────────────────────────────────────────

public record StoredApplication(
    string ApplicationId,
    ApplicationInput Input,
    ScreeningResult Result,
    DateTime SubmittedAt,
    ApplicationStatus Status = ApplicationStatus.Screened,
    DateTime? StatusChangedAt = null,
    string? OfficerNotes = null,
    PaymentStatus PaymentStatus = PaymentStatus.Paid,   // default Paid for seed/demo data
    decimal FeeAmount = 0m,
    string? PaymentReference = null
);

// ── Officer AI Analysis Chat ──────────────────────────────────────────────

public enum ChatRole { Assistant, User }
public record ChatMessage(ChatRole Role, string Content, DateTime Timestamp);

public enum SourceType { Text, Url, File }
public record AnalysisSource(SourceType Type, string Title, string Content);

// ── Application lifecycle status ──────────────────────────────────────────

public enum ApplicationStatus
{
    Screened,              // AI pre-screening complete, waiting for officer
    UnderReview,           // Officer actively reviewing
    SupplementRequested,   // Officer requested additional documents
    SupplementReceived,    // Applicant responded to supplement request
    Approved,              // Officer approved — moving to biometrics
    AwaitingBiometrics,    // Applicant must visit Migri for fingerprints/photo
    BiometricsComplete,    // Biometrics collected at service point
    PermitInProduction,    // Physical permit card being printed (~2-3 weeks)
    ReadyForCollection,    // Permit card ready to collect at service point
    PermitIssued,          // Permit delivered/collected — process complete
    Rejected,              // Application rejected
}

// ── Audit log entry ───────────────────────────────────────────────────────

public record ChatAuditEntry(
    string ApplicationId,
    DateTime Timestamp,
    string UserQuery,
    string AiResponse
);

// ── Official message thread (Officer ↔ Applicant) ────────────────────────

public enum OfficialMessageType
{
    GeneralMessage,        // Officer free-form message
    SupplementRequest,     // Formal supplement request with document list
    StatusNotification,    // Auto-generated on key status changes
    ProfileAccessNotice,   // Auto-generated when officer opens a file (GDPR notice)
    ApplicantReply,        // Applicant's reply / supplement submission
}

public record OfficialMessage(
    string MessageId,
    string ApplicationId,
    OfficialMessageType Type,
    string SenderRole,           // "officer" | "applicant" | "system"
    string Content,
    DateTime SentAt,
    bool ReadByOfficer,
    bool ReadByApplicant,
    string? AttachmentFileName = null
);

// ── Profile access audit (GDPR Article 5 — purpose limitation) ───────────

public record ProfileAccessEntry(
    string ApplicationId,
    DateTime AccessedAt,
    string Reason
);
