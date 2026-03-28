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
public record GenericDocAnalysis(bool IsRelevant, string DocumentType, string Notes);

// ── Permit category system ────────────────────────────────────────────────

public enum PermitGroup { Work, Study, Family }

public enum PermitType { Student, Work }  // kept for backward compat

public enum PermitCategory
{
    // Work-Based (Työ) 1-9
    EmployeePermit,        // TTOL — Employee's residence permit
    SpecialistExpert,      // Specialist / expert permit
    EUBlueCard,            // EU Blue Card
    IntraCorporate,        // Intra-corporate transferee (ICT)
    SeasonalWorker,        // Seasonal worker
    Researcher,            // Researcher
    SelfEmployed,          // Self-employed / entrepreneur
    AuPair,                // Au pair
    WorkingHoliday,        // Working holiday permit

    // Study-Based (Opiskelu) 10-14
    StudentHigherEd,       // Student — higher education
    StudentVocational,     // Student — vocational education
    LanguageCourse,        // Language course / short-term study
    ExchangeStudent,       // Exchange student
    TraineeIntern,         // Trainee / intern

    // Family (Perhe) 15-22
    SpouseOfFinnish,       // Spouse / registered partner of Finnish citizen
    SpouseOfEUCitizen,     // Spouse / partner of EU citizen
    SpouseOfPermitHolder,  // Spouse / partner of permit holder
    ChildOfFinnish,        // Child of Finnish citizen
    ChildOfPermitHolder,   // Child of permit holder
    ParentOfMinorFinnish,  // Parent of minor Finnish citizen
    OtherFamilyEU,         // Other family member (EU rules)
    DependentFamily        // Dependent family member
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
    DateTime? PassportExpiry
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

// ── Stored Application ────────────────────────────────────────────────────

public record StoredApplication(
    string ApplicationId,
    ApplicationInput Input,
    ScreeningResult Result,
    DateTime SubmittedAt,
    ApplicationStatus Status = ApplicationStatus.Screened,
    DateTime? StatusChangedAt = null,
    string? OfficerNotes = null
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
