namespace PermitReady;

public record RuleField(string FieldName, string Label, int Points);
public record RuleDoc(string Keyword, string Label, int Points);
public record PermitRule(
    string TypeLabel,
    List<RuleField> Fields,
    List<RuleDoc> Docs,
    decimal? MinFunds,
    decimal? MinSalary
);

public static class PermitCategoryExtensions
{
    public static PermitGroup GetGroup(this PermitCategory cat) => cat switch
    {
        // Work (30)
        PermitCategory.EmployeePermit or PermitCategory.SpecialistExpert or
        PermitCategory.EUBlueCard or PermitCategory.IntraCorporate or
        PermitCategory.StartupEntrepreneur or PermitCategory.SelfEmployed or
        PermitCategory.TopMiddleManagement or PermitCategory.Researcher or
        PermitCategory.CulturalArtsWork or PermitCategory.MassMediaWork or
        PermitCategory.ReligiousCommunityWorker or PermitCategory.Athlete or
        PermitCategory.Coach or PermitCategory.InternationalOrgWork or
        PermitCategory.SeasonalWorker or PermitCategory.AuPair or
        PermitCategory.WorkingHoliday or PermitCategory.InternshipProgram or
        PermitCategory.VolunteerWork or PermitCategory.CompanyPreparation or
        PermitCategory.LookForWork or PermitCategory.DegreeCompletedInFinland or
        PermitCategory.ResearchCompletedInFinland or PermitCategory.Remigration or
        PermitCategory.MachineryDelivery or PermitCategory.LanguageTeacher or
        PermitCategory.HealthcareWorker or PermitCategory.TechSpecialist or
        PermitCategory.AgricultureWorker or PermitCategory.ConstructionWorker => PermitGroup.Work,

        // Study (10)
        PermitCategory.StudentHigherEd or PermitCategory.StudentVocational or
        PermitCategory.StudentPHD or PermitCategory.ExchangeStudent or
        PermitCategory.LanguageCourse or PermitCategory.TraineeIntern or
        PermitCategory.PostDocResearcher or PermitCategory.UniversityResearch or
        PermitCategory.TrainingProgram or PermitCategory.ApprenticeSummer => PermitGroup.Study,

        // Family (10)
        PermitCategory.SpouseOfFinnish or PermitCategory.SpouseOfEUCitizen or
        PermitCategory.SpouseOfPermitHolder or PermitCategory.ChildOfFinnish or
        PermitCategory.ChildOfPermitHolder or PermitCategory.ChildOfEUCitizen or
        PermitCategory.ParentOfMinorFinnish or PermitCategory.ParentOfMinorPermitHolder or
        PermitCategory.OtherFamilyEU or PermitCategory.DependentFamily => PermitGroup.Family,

        // Protection (4)
        PermitCategory.Asylum or PermitCategory.TemporaryProtection or
        PermitCategory.HumanTraffickingVictim or PermitCategory.CompassionateGrounds => PermitGroup.Protection,

        // Other (5)
        _ => PermitGroup.Other
    };

    public static string DisplayName(this PermitCategory cat) => cat switch
    {
        PermitCategory.EmployeePermit       => "Employee's Residence Permit (TTOL)",
        PermitCategory.SpecialistExpert     => "Specialist / Expert Permit",
        PermitCategory.EUBlueCard           => "EU Blue Card",
        PermitCategory.IntraCorporate       => "Intra-Corporate Transferee (ICT)",
        PermitCategory.SeasonalWorker       => "Seasonal Worker Permit",
        PermitCategory.Researcher           => "Researcher Permit",
        PermitCategory.SelfEmployed         => "Self-Employed / Entrepreneur Permit",
        PermitCategory.AuPair               => "Au Pair Permit",
        PermitCategory.WorkingHoliday       => "Working Holiday Permit",
        PermitCategory.StudentHigherEd      => "Student — Higher Education",
        PermitCategory.StudentVocational    => "Student — Vocational Education",
        PermitCategory.LanguageCourse       => "Language Course / Short-Term Study",
        PermitCategory.ExchangeStudent      => "Exchange Student",
        PermitCategory.TraineeIntern        => "Trainee / Intern",
        PermitCategory.SpouseOfFinnish      => "Spouse / Partner of Finnish Citizen",
        PermitCategory.SpouseOfEUCitizen    => "Spouse / Partner of EU Citizen",
        PermitCategory.SpouseOfPermitHolder => "Spouse / Partner of Permit Holder",
        PermitCategory.ChildOfFinnish       => "Child of Finnish Citizen",
        PermitCategory.ChildOfPermitHolder  => "Child of Permit Holder",
        PermitCategory.ParentOfMinorFinnish => "Parent of Minor Finnish Citizen",
        PermitCategory.ParentOfMinorPermitHolder => "Parent of Minor Permit Holder",
        PermitCategory.OtherFamilyEU        => "Other Family Member (EU Rules)",
        PermitCategory.DependentFamily      => "Dependent Family Member",

        // New work categories
        PermitCategory.StartupEntrepreneur  => "Start-up Entrepreneur Permit",
        PermitCategory.TopMiddleManagement  => "Top & Middle Management",
        PermitCategory.CulturalArtsWork     => "Cultural or Arts Work",
        PermitCategory.MassMediaWork        => "Mass Media Work",
        PermitCategory.ReligiousCommunityWorker => "Religious Community Employee",
        PermitCategory.Athlete              => "Athlete",
        PermitCategory.Coach                => "Coach or Trainer",
        PermitCategory.InternationalOrgWork => "International Organization Work",
        PermitCategory.InternshipProgram    => "Internship Programme",
        PermitCategory.VolunteerWork        => "Volunteer Work",
        PermitCategory.CompanyPreparation   => "Company Preparation Work",
        PermitCategory.LookForWork          => "Look for Work (Post-Graduation)",
        PermitCategory.DegreeCompletedInFinland => "Degree-Completed-in-Finland Work",
        PermitCategory.ResearchCompletedInFinland => "Research-Completed-in-Finland Work",
        PermitCategory.Remigration          => "Remigration Permit",
        PermitCategory.MachineryDelivery    => "Machinery/System Delivery",
        PermitCategory.LanguageTeacher      => "Language Teacher",
        PermitCategory.HealthcareWorker     => "Healthcare Worker",
        PermitCategory.TechSpecialist       => "Tech Specialist",
        PermitCategory.AgricultureWorker    => "Agricultural Worker",
        PermitCategory.ConstructionWorker   => "Construction Worker",

        // New study categories
        PermitCategory.StudentPHD           => "PhD/Doctoral Student",
        PermitCategory.PostDocResearcher    => "Post-Doctoral Researcher",
        PermitCategory.UniversityResearch   => "University Research Programme",
        PermitCategory.TrainingProgram      => "Training/Preparation Programme",
        PermitCategory.ApprenticeSummer     => "Apprentice/Summer Student",
        PermitCategory.ChildOfEUCitizen     => "Child of EU Citizen",

        // New protection & other categories
        PermitCategory.Asylum               => "Asylum/International Protection",
        PermitCategory.TemporaryProtection  => "Temporary Protection",
        PermitCategory.HumanTraffickingVictim => "Human Trafficking Victim",
        PermitCategory.CompassionateGrounds => "Residence on Compassionate Grounds",
        PermitCategory.PermanentResidence   => "Permanent Residence Permit",
        PermitCategory.EUCitizenRegistration => "EU Citizen Registration",
        PermitCategory.ReturningResident    => "Returning Resident (D Visa)",
        PermitCategory.Citizenship          => "Finnish Citizenship",
        PermitCategory.VisitingResearcher   => "Visiting Researcher",

        _                                   => cat.ToString()
    };

    public static string ShortName(this PermitCategory cat) => cat switch
    {
        PermitCategory.EmployeePermit       => "Employee (TTOL)",
        PermitCategory.SpecialistExpert     => "Specialist",
        PermitCategory.EUBlueCard           => "EU Blue Card",
        PermitCategory.IntraCorporate       => "ICT",
        PermitCategory.SeasonalWorker       => "Seasonal",
        PermitCategory.Researcher           => "Researcher",
        PermitCategory.SelfEmployed         => "Self-Employed",
        PermitCategory.AuPair               => "Au Pair",
        PermitCategory.WorkingHoliday       => "Working Holiday",
        PermitCategory.StudentHigherEd      => "Higher Ed",
        PermitCategory.StudentVocational    => "Vocational",
        PermitCategory.LanguageCourse       => "Language Course",
        PermitCategory.ExchangeStudent      => "Exchange",
        PermitCategory.TraineeIntern        => "Trainee",
        PermitCategory.SpouseOfFinnish      => "Spouse of Finnish",
        PermitCategory.SpouseOfEUCitizen    => "Spouse of EU",
        PermitCategory.SpouseOfPermitHolder => "Spouse of Holder",
        PermitCategory.ChildOfFinnish       => "Child of Finnish",
        PermitCategory.ChildOfPermitHolder  => "Child of Holder",
        PermitCategory.ParentOfMinorFinnish => "Parent of Finnish",
        PermitCategory.ParentOfMinorPermitHolder => "Parent of Holder",
        PermitCategory.OtherFamilyEU        => "Family (EU)",
        PermitCategory.DependentFamily      => "Dependent",

        // New work
        PermitCategory.StartupEntrepreneur  => "Startup",
        PermitCategory.TopMiddleManagement  => "Management",
        PermitCategory.CulturalArtsWork     => "Arts",
        PermitCategory.MassMediaWork        => "Media",
        PermitCategory.ReligiousCommunityWorker => "Religious",
        PermitCategory.Athlete              => "Athlete",
        PermitCategory.Coach                => "Coach",
        PermitCategory.InternationalOrgWork => "Intl Org",
        PermitCategory.InternshipProgram    => "Internship",
        PermitCategory.VolunteerWork        => "Volunteer",
        PermitCategory.CompanyPreparation   => "Prep Work",
        PermitCategory.LookForWork          => "Work Search",
        PermitCategory.DegreeCompletedInFinland => "Degree (FI)",
        PermitCategory.ResearchCompletedInFinland => "Research (FI)",
        PermitCategory.Remigration          => "Remigration",
        PermitCategory.MachineryDelivery    => "Machinery",
        PermitCategory.LanguageTeacher      => "Teacher",
        PermitCategory.HealthcareWorker     => "Healthcare",
        PermitCategory.TechSpecialist       => "Tech",
        PermitCategory.AgricultureWorker    => "Agriculture",
        PermitCategory.ConstructionWorker   => "Construction",

        // New study
        PermitCategory.StudentPHD           => "PhD Student",
        PermitCategory.PostDocResearcher    => "Post-Doc",
        PermitCategory.UniversityResearch   => "Research (Uni)",
        PermitCategory.TrainingProgram      => "Training",
        PermitCategory.ApprenticeSummer     => "Apprentice",
        PermitCategory.ChildOfEUCitizen     => "Child (EU)",

        // Protection & other
        PermitCategory.Asylum               => "Asylum",
        PermitCategory.TemporaryProtection  => "Temp Protection",
        PermitCategory.HumanTraffickingVictim => "Trafficking Victim",
        PermitCategory.CompassionateGrounds => "Compassionate",
        PermitCategory.PermanentResidence   => "Permanent",
        PermitCategory.EUCitizenRegistration => "EU Citizen",
        PermitCategory.ReturningResident    => "Returning",
        PermitCategory.Citizenship          => "Citizenship",
        PermitCategory.VisitingResearcher   => "Visiting Scholar",

        _                                   => cat.ToString()
    };

    public static string MigriFiUrl(this PermitCategory cat) => cat switch
    {
        PermitCategory.EmployeePermit       => "https://migri.fi/en/employee-s-residence-permit",
        PermitCategory.SpecialistExpert     => "https://migri.fi/en/specialist",
        PermitCategory.EUBlueCard           => "https://migri.fi/en/eu-blue-card",
        PermitCategory.IntraCorporate       => "https://migri.fi/en/intra-corporate-transferee",
        PermitCategory.SeasonalWorker       => "https://migri.fi/en/seasonal-work",
        PermitCategory.Researcher           => "https://migri.fi/en/researcher",
        PermitCategory.SelfEmployed         => "https://migri.fi/en/self-employed-person",
        PermitCategory.AuPair               => "https://migri.fi/en/au-pair",
        PermitCategory.WorkingHoliday       => "https://migri.fi/en/working-holiday",
        PermitCategory.StudentHigherEd      => "https://migri.fi/en/student",
        PermitCategory.StudentVocational    => "https://migri.fi/en/student",
        PermitCategory.LanguageCourse       => "https://migri.fi/en/student",
        PermitCategory.ExchangeStudent      => "https://migri.fi/en/student",
        PermitCategory.TraineeIntern        => "https://migri.fi/en/trainee",
        PermitCategory.SpouseOfFinnish      => "https://migri.fi/en/family-member-of-a-finnish-citizen",
        PermitCategory.SpouseOfEUCitizen    => "https://migri.fi/en/family-member-of-an-eu-citizen",
        PermitCategory.SpouseOfPermitHolder => "https://migri.fi/en/family-member-of-a-person-residing-in-finland",
        PermitCategory.ChildOfFinnish       => "https://migri.fi/en/family-member-of-a-finnish-citizen",
        PermitCategory.ChildOfPermitHolder  => "https://migri.fi/en/family-member-of-a-person-residing-in-finland",
        PermitCategory.ParentOfMinorFinnish => "https://migri.fi/en/family-member-of-a-finnish-citizen",
        PermitCategory.ParentOfMinorPermitHolder => "https://migri.fi/en/family-member-of-a-person-residing-in-finland",
        PermitCategory.OtherFamilyEU        => "https://migri.fi/en/family-member-of-an-eu-citizen",
        PermitCategory.DependentFamily      => "https://migri.fi/en/family-member-of-a-person-residing-in-finland",

        // New work categories
        PermitCategory.StartupEntrepreneur  => "https://migri.fi/en/start-up-entrepreneur",
        PermitCategory.TopMiddleManagement  => "https://migri.fi/en/specialist",
        PermitCategory.CulturalArtsWork     => "https://migri.fi/en/cultural-or-sports-work",
        PermitCategory.MassMediaWork        => "https://migri.fi/en/mass-media-work",
        PermitCategory.ReligiousCommunityWorker => "https://migri.fi/en/employee-of-a-religious-community",
        PermitCategory.Athlete              => "https://migri.fi/en/athlete",
        PermitCategory.Coach                => "https://migri.fi/en/coach-or-sports-manager",
        PermitCategory.InternationalOrgWork => "https://migri.fi/en/work-for-international-organisation",
        PermitCategory.InternshipProgram    => "https://migri.fi/en/trainee",
        PermitCategory.VolunteerWork        => "https://migri.fi/en/volunteer",
        PermitCategory.CompanyPreparation   => "https://migri.fi/en/company-preparation-and-supervision",
        PermitCategory.LookForWork          => "https://migri.fi/en/residence-permit-to-look-for-work",
        PermitCategory.DegreeCompletedInFinland => "https://migri.fi/en/degree-completed-in-finland",
        PermitCategory.ResearchCompletedInFinland => "https://migri.fi/en/research-completed-in-finland",
        PermitCategory.Remigration          => "https://migri.fi/en/remigration",
        PermitCategory.MachineryDelivery    => "https://migri.fi/en/deliveries-of-machinery-and-systems",
        PermitCategory.LanguageTeacher      => "https://migri.fi/en/specialist",
        PermitCategory.HealthcareWorker     => "https://migri.fi/en/employee-s-residence-permit",
        PermitCategory.TechSpecialist       => "https://migri.fi/en/specialist",
        PermitCategory.AgricultureWorker    => "https://migri.fi/en/seasonal-work",
        PermitCategory.ConstructionWorker   => "https://migri.fi/en/employee-s-residence-permit",

        // New study categories
        PermitCategory.StudentPHD           => "https://migri.fi/en/student",
        PermitCategory.PostDocResearcher    => "https://migri.fi/en/researcher",
        PermitCategory.UniversityResearch   => "https://migri.fi/en/researcher",
        PermitCategory.TrainingProgram      => "https://migri.fi/en/student",
        PermitCategory.ApprenticeSummer     => "https://migri.fi/en/student",
        PermitCategory.ChildOfEUCitizen     => "https://migri.fi/en/family-member-of-an-eu-citizen",

        // Protection & other categories
        PermitCategory.Asylum               => "https://migri.fi/en/asylum",
        PermitCategory.TemporaryProtection  => "https://migri.fi/en/temporary-protection",
        PermitCategory.HumanTraffickingVictim => "https://migri.fi/en/human-trafficking-victim",
        PermitCategory.CompassionateGrounds => "https://migri.fi/en/residence-permit",
        PermitCategory.PermanentResidence   => "https://migri.fi/en/permanent-residence-permit",
        PermitCategory.EUCitizenRegistration => "https://migri.fi/en/eu-citizen-registration-of-residence",
        PermitCategory.ReturningResident    => "https://migri.fi/en/returning-resident",
        PermitCategory.Citizenship          => "https://migri.fi/en/citizenship",
        PermitCategory.VisitingResearcher   => "https://migri.fi/en/researcher",

        _                                   => "https://migri.fi/en/residence-permits"
    };

    public static string GroupIcon(this PermitGroup group) => group switch
    {
        PermitGroup.Work       => "briefcase",
        PermitGroup.Study      => "graduation-cap",
        PermitGroup.Family     => "heart",
        PermitGroup.Protection => "shield",
        PermitGroup.Other      => "file",
        _                      => "file"
    };

    public static string CategoryIcon(this PermitCategory cat) => cat switch
    {
        PermitCategory.EmployeePermit       => "hard-hat",
        PermitCategory.SpecialistExpert     => "award",
        PermitCategory.EUBlueCard           => "credit-card",
        PermitCategory.IntraCorporate       => "building-2",
        PermitCategory.SeasonalWorker       => "sun",
        PermitCategory.Researcher           => "microscope",
        PermitCategory.SelfEmployed         => "store",
        PermitCategory.AuPair               => "baby",
        PermitCategory.WorkingHoliday       => "plane",
        PermitCategory.StudentHigherEd      => "university",
        PermitCategory.StudentVocational    => "wrench",
        PermitCategory.LanguageCourse       => "languages",
        PermitCategory.ExchangeStudent      => "repeat",
        PermitCategory.TraineeIntern        => "user-check",
        PermitCategory.SpouseOfFinnish      => "ring",
        PermitCategory.SpouseOfEUCitizen    => "heart-handshake",
        PermitCategory.SpouseOfPermitHolder => "users",
        PermitCategory.ChildOfFinnish       => "baby",
        PermitCategory.ChildOfPermitHolder  => "baby",
        PermitCategory.ParentOfMinorFinnish => "user-round",
        PermitCategory.ParentOfMinorPermitHolder => "user-round",
        PermitCategory.OtherFamilyEU        => "users",
        PermitCategory.DependentFamily      => "heart",

        // New work
        PermitCategory.StartupEntrepreneur  => "zap",
        PermitCategory.TopMiddleManagement  => "crown",
        PermitCategory.CulturalArtsWork     => "palette",
        PermitCategory.MassMediaWork        => "tv",
        PermitCategory.ReligiousCommunityWorker => "cross",
        PermitCategory.Athlete              => "activity",
        PermitCategory.Coach                => "target",
        PermitCategory.InternationalOrgWork => "globe",
        PermitCategory.InternshipProgram    => "user-check",
        PermitCategory.VolunteerWork        => "heart-handshake",
        PermitCategory.CompanyPreparation   => "clipboard",
        PermitCategory.LookForWork          => "search",
        PermitCategory.DegreeCompletedInFinland => "scroll",
        PermitCategory.ResearchCompletedInFinland => "scroll",
        PermitCategory.Remigration          => "arrow-right-circle",
        PermitCategory.MachineryDelivery    => "package",
        PermitCategory.LanguageTeacher      => "languages",
        PermitCategory.HealthcareWorker     => "ambulance",
        PermitCategory.TechSpecialist       => "code",
        PermitCategory.AgricultureWorker    => "leaf",
        PermitCategory.ConstructionWorker   => "hammer",

        // New study
        PermitCategory.StudentPHD           => "book-open",
        PermitCategory.PostDocResearcher    => "microscope",
        PermitCategory.UniversityResearch   => "beaker",
        PermitCategory.TrainingProgram      => "clipboard-list",
        PermitCategory.ApprenticeSummer     => "briefcase",
        PermitCategory.ChildOfEUCitizen     => "baby",

        // Protection & other
        PermitCategory.Asylum               => "shield-alert",
        PermitCategory.TemporaryProtection  => "shield-check",
        PermitCategory.HumanTraffickingVictim => "shield-x",
        PermitCategory.CompassionateGrounds => "hand-heart",
        PermitCategory.PermanentResidence   => "home",
        PermitCategory.EUCitizenRegistration => "passport",
        PermitCategory.ReturningResident    => "log-in",
        PermitCategory.Citizenship          => "flag",
        PermitCategory.VisitingResearcher   => "user-plus",

        _                                   => "file"
    };
}

public static class PermitRules
{
    // ── Rules per category ────────────────────────────────────────────────

    private static readonly Dictionary<PermitCategory, PermitRule> Rules = new()
    {
        [PermitCategory.EmployeePermit] = new("Employee's Residence Permit (TTOL)",
            Fields:
            [
                new("FullName",              "Full name",              5),
                new("Nationality",           "Nationality",            5),
                new("Email",                 "Email",                  5),
                new("EmployerName",          "Employer name",          10),
                new("JobTitle",              "Job title",              10),
                new("SalaryAmount",          "Monthly salary",         15),
                new("EmploymentContractRef", "Employment contract ref", 5),
                new("PassportExpiry",        "Passport expiry",        5),
            ],
            Docs:
            [
                new("contract", "Employment contract", 15),
                new("passport", "Passport copy",       15),
                new("salary",   "Salary proof",        10),
            ],
            MinFunds: null, MinSalary: 1500m),

        [PermitCategory.SpecialistExpert] = new("Specialist / Expert Permit",
            Fields:
            [
                new("FullName",              "Full name",              5),
                new("Nationality",           "Nationality",            5),
                new("Email",                 "Email",                  5),
                new("EmployerName",          "Employer name",          10),
                new("JobTitle",              "Job title",              10),
                new("SalaryAmount",          "Monthly salary",         15),
                new("EmploymentContractRef", "Employment contract ref", 5),
                new("PassportExpiry",        "Passport expiry",        5),
            ],
            Docs:
            [
                new("contract", "Employment contract", 10),
                new("passport", "Passport copy",       15),
                new("degree",   "Degree certificate",  10),
                new("salary",   "Salary proof",        10),
            ],
            MinFunds: null, MinSalary: 3000m),

        [PermitCategory.EUBlueCard] = new("EU Blue Card",
            Fields:
            [
                new("FullName",              "Full name",              5),
                new("Nationality",           "Nationality",            5),
                new("Email",                 "Email",                  5),
                new("EmployerName",          "Employer name",          10),
                new("JobTitle",              "Job title",              10),
                new("SalaryAmount",          "Monthly salary",         15),
                new("EmploymentContractRef", "Employment contract ref", 5),
                new("PassportExpiry",        "Passport expiry",        5),
            ],
            Docs:
            [
                new("contract", "Employment contract", 10),
                new("passport", "Passport copy",       10),
                new("degree",   "Degree certificate",  10),
                new("salary",   "Salary proof",        10),
            ],
            MinFunds: null, MinSalary: 4867m),

        [PermitCategory.IntraCorporate] = new("Intra-Corporate Transferee (ICT)",
            Fields:
            [
                new("FullName",              "Full name",              5),
                new("Nationality",           "Nationality",            5),
                new("Email",                 "Email",                  5),
                new("EmployerName",          "Employer name",          15),
                new("JobTitle",              "Job title",              10),
                new("SalaryAmount",          "Monthly salary",         10),
                new("EmploymentContractRef", "Employment contract ref", 10),
                new("PassportExpiry",        "Passport expiry",        5),
            ],
            Docs:
            [
                new("assignment", "Assignment letter",   15),
                new("passport",   "Passport copy",       15),
                new("contract",   "Employment contract", 10),
                new("company",    "Company documents",   10),
            ],
            MinFunds: null, MinSalary: 3000m),

        [PermitCategory.SeasonalWorker] = new("Seasonal Worker Permit",
            Fields:
            [
                new("FullName",              "Full name",              5),
                new("Nationality",           "Nationality",            5),
                new("Email",                 "Email",                  5),
                new("EmployerName",          "Employer name",          15),
                new("JobTitle",              "Job title",              10),
                new("SalaryAmount",          "Monthly salary",         10),
                new("EmploymentContractRef", "Employment contract ref", 10),
                new("PassportExpiry",        "Passport expiry",        5),
            ],
            Docs:
            [
                new("contract",       "Employment contract", 20),
                new("passport",       "Passport copy",       15),
                new("accommodation",  "Accommodation proof", 10),
            ],
            MinFunds: null, MinSalary: 1200m),

        [PermitCategory.Researcher] = new("Researcher Permit",
            Fields:
            [
                new("FullName",       "Full name",     5),
                new("Nationality",    "Nationality",   5),
                new("Email",          "Email",         5),
                new("EmployerName",   "Employer name", 15),
                new("JobTitle",       "Job title",     5),
                new("SalaryAmount",   "Monthly salary", 10),
                new("PassportExpiry", "Passport expiry", 5),
            ],
            Docs:
            [
                new("hosting",  "Hosting agreement",   15),
                new("passport", "Passport copy",       15),
                new("research", "Research agreement",  15),
                new("funding",  "Funding confirmation", 10),
            ],
            MinFunds: null, MinSalary: 1500m),

        [PermitCategory.SelfEmployed] = new("Self-Employed / Entrepreneur Permit",
            Fields:
            [
                new("FullName",       "Full name",     5),
                new("Nationality",    "Nationality",   5),
                new("Email",          "Email",         5),
                new("EmployerName",   "Business name", 15),
                new("JobTitle",       "Business type", 5),
                new("SalaryAmount",   "Monthly income", 10),
                new("PassportExpiry", "Passport expiry", 5),
            ],
            Docs:
            [
                new("business", "Business registration", 20),
                new("passport", "Passport copy",         15),
                new("tax",      "Tax registration",      10),
                new("bank",     "Bank statement",        10),
            ],
            MinFunds: null, MinSalary: 1000m),

        [PermitCategory.AuPair] = new("Au Pair Permit",
            Fields:
            [
                new("FullName",       "Full name",        5),
                new("Nationality",    "Nationality",      5),
                new("Email",          "Email",            5),
                new("EmployerName",   "Host family name", 10),
                new("SalaryAmount",   "Monthly allowance", 10),
                new("PassportExpiry", "Passport expiry",  5),
            ],
            Docs:
            [
                new("au_pair",      "Au pair agreement",    20),
                new("passport",     "Passport copy",        15),
                new("host",         "Host family documents", 15),
                new("accommodation","Accommodation proof",   10),
            ],
            MinFunds: null, MinSalary: 450m),

        [PermitCategory.WorkingHoliday] = new("Working Holiday Permit",
            Fields:
            [
                new("FullName",       "Full name",       5),
                new("Nationality",    "Nationality",     5),
                new("Email",          "Email",           5),
                new("FundsAmount",    "Available funds", 20),
                new("PassportExpiry", "Passport expiry", 10),
            ],
            Docs:
            [
                new("insurance", "Travel insurance", 20),
                new("passport",  "Passport copy",    15),
                new("bank",      "Bank statement",   10),
            ],
            MinFunds: 500m, MinSalary: null),

        [PermitCategory.StudentHigherEd] = new("Student — Higher Education",
            Fields:
            [
                new("FullName",        "Full name",      5),
                new("Nationality",     "Nationality",    5),
                new("Email",           "Email",          5),
                new("UniversityName",  "University name", 10),
                new("ProgramName",     "Programme name", 10),
                new("FundsAmount",     "Monthly funds",  15),
                new("PassportExpiry",  "Passport expiry", 10),
            ],
            Docs:
            [
                new("acceptance", "Acceptance letter",   15),
                new("transcript", "Academic transcript", 10),
                new("passport",   "Passport copy",       15),
            ],
            MinFunds: 560m, MinSalary: null),

        [PermitCategory.StudentVocational] = new("Student — Vocational Education",
            Fields:
            [
                new("FullName",       "Full name",       5),
                new("Nationality",    "Nationality",     5),
                new("Email",          "Email",           5),
                new("UniversityName", "Institution name", 10),
                new("ProgramName",    "Programme name",  10),
                new("FundsAmount",    "Monthly funds",   15),
                new("PassportExpiry", "Passport expiry", 10),
            ],
            Docs:
            [
                new("acceptance", "Acceptance letter",   15),
                new("transcript", "Academic transcript", 10),
                new("passport",   "Passport copy",       15),
            ],
            MinFunds: 560m, MinSalary: null),

        [PermitCategory.LanguageCourse] = new("Language Course / Short-Term Study",
            Fields:
            [
                new("FullName",       "Full name",       5),
                new("Nationality",    "Nationality",     5),
                new("Email",          "Email",           5),
                new("UniversityName", "Institution name", 15),
                new("FundsAmount",    "Monthly funds",   20),
                new("PassportExpiry", "Passport expiry", 10),
            ],
            Docs:
            [
                new("enrollment", "Enrollment confirmation", 20),
                new("passport",   "Passport copy",           15),
                new("bank",       "Bank statement",          10),
            ],
            MinFunds: 560m, MinSalary: null),

        [PermitCategory.ExchangeStudent] = new("Exchange Student",
            Fields:
            [
                new("FullName",       "Full name",        5),
                new("Nationality",    "Nationality",      5),
                new("Email",          "Email",            5),
                new("UniversityName", "Host university",  10),
                new("ProgramName",    "Exchange programme", 10),
                new("FundsAmount",    "Monthly funds",    10),
                new("PassportExpiry", "Passport expiry",  10),
            ],
            Docs:
            [
                new("exchange",   "Exchange agreement",    15),
                new("acceptance", "Acceptance letter",     10),
                new("passport",   "Passport copy",         15),
                new("home",       "Home university letter", 10),
            ],
            MinFunds: 560m, MinSalary: null),

        [PermitCategory.TraineeIntern] = new("Trainee / Intern",
            Fields:
            [
                new("FullName",       "Full name",      5),
                new("Nationality",    "Nationality",    5),
                new("Email",          "Email",          5),
                new("EmployerName",   "Employer name",  10),
                new("JobTitle",       "Position title", 10),
                new("FundsAmount",    "Monthly funds",  10),
                new("PassportExpiry", "Passport expiry", 10),
            ],
            Docs:
            [
                new("training",   "Training agreement", 20),
                new("passport",   "Passport copy",      15),
                new("supervisor", "Supervisor letter",  10),
                new("bank",       "Bank statement",     10),
            ],
            MinFunds: 560m, MinSalary: null),

        [PermitCategory.SpouseOfFinnish] = new("Spouse / Partner of Finnish Citizen",
            Fields:
            [
                new("FullName",             "Full name",    5),
                new("Nationality",          "Nationality",  5),
                new("Email",                "Email",        5),
                new("SponsorName",          "Sponsor name", 15),
                new("SponsorPermitNumber",  "Sponsor ID",   10),
                new("PassportExpiry",       "Passport expiry", 10),
            ],
            Docs:
            [
                new("marriage",  "Marriage certificate", 20),
                new("passport",  "Passport copy",        15),
                new("sponsor",   "Sponsor identity",     10),
                new("identity",  "Identity documents",   10),
            ],
            MinFunds: null, MinSalary: null),

        [PermitCategory.SpouseOfEUCitizen] = new("Spouse / Partner of EU Citizen",
            Fields:
            [
                new("FullName",            "Full name",       5),
                new("Nationality",         "Nationality",     5),
                new("Email",               "Email",           5),
                new("SponsorName",         "Sponsor name",    15),
                new("SponsorPermitNumber", "EU permit number", 10),
                new("PassportExpiry",      "Passport expiry", 10),
            ],
            Docs:
            [
                new("marriage",   "Marriage certificate",  20),
                new("passport",   "Passport copy",         15),
                new("eu_permit",  "EU residence permit",   10),
                new("identity",   "Identity documents",    10),
            ],
            MinFunds: null, MinSalary: null),

        [PermitCategory.SpouseOfPermitHolder] = new("Spouse / Partner of Permit Holder",
            Fields:
            [
                new("FullName",            "Full name",           5),
                new("Nationality",         "Nationality",         5),
                new("Email",               "Email",               5),
                new("SponsorName",         "Sponsor name",        15),
                new("SponsorPermitNumber", "Sponsor permit number", 15),
                new("PassportExpiry",      "Passport expiry",     10),
            ],
            Docs:
            [
                new("marriage", "Marriage certificate",   20),
                new("passport", "Passport copy",          15),
                new("sponsor",  "Sponsor's permit copy",  15),
            ],
            MinFunds: null, MinSalary: null),

        [PermitCategory.ChildOfFinnish] = new("Child of Finnish Citizen",
            Fields:
            [
                new("FullName",       "Full name",    5),
                new("Nationality",    "Nationality",  5),
                new("Email",          "Email",        5),
                new("SponsorName",    "Parent name",  15),
                new("PassportExpiry", "Passport expiry", 10),
            ],
            Docs:
            [
                new("birth",    "Birth certificate",  20),
                new("passport", "Passport copy",      15),
                new("parent",   "Parent's identity",  15),
                new("custody",  "Custody documents",  10),
            ],
            MinFunds: null, MinSalary: null),

        [PermitCategory.ChildOfPermitHolder] = new("Child of Permit Holder",
            Fields:
            [
                new("FullName",            "Full name",          5),
                new("Nationality",         "Nationality",        5),
                new("Email",               "Email",              5),
                new("SponsorName",         "Parent name",        15),
                new("SponsorPermitNumber", "Parent permit number", 10),
                new("PassportExpiry",      "Passport expiry",    10),
            ],
            Docs:
            [
                new("birth",         "Birth certificate",    20),
                new("passport",      "Passport copy",        15),
                new("parent_permit", "Parent's permit copy", 15),
            ],
            MinFunds: null, MinSalary: null),

        [PermitCategory.ParentOfMinorFinnish] = new("Parent of Minor Finnish Citizen",
            Fields:
            [
                new("FullName",       "Full name",    5),
                new("Nationality",    "Nationality",  5),
                new("Email",          "Email",        5),
                new("SponsorName",    "Child's name", 15),
                new("PassportExpiry", "Passport expiry", 10),
            ],
            Docs:
            [
                new("birth",          "Birth certificate",  15),
                new("passport",       "Passport copy",      15),
                new("custody",        "Custody documents",  15),
                new("child_passport", "Child's passport",   10),
            ],
            MinFunds: null, MinSalary: null),

        [PermitCategory.OtherFamilyEU] = new("Other Family Member (EU Rules)",
            Fields:
            [
                new("FullName",            "Full name",            5),
                new("Nationality",         "Nationality",          5),
                new("Email",               "Email",                5),
                new("SponsorName",         "EU family member name", 15),
                new("SponsorPermitNumber", "EU permit number",     10),
                new("PassportExpiry",      "Passport expiry",      10),
            ],
            Docs:
            [
                new("relationship", "Relationship proof",    20),
                new("passport",     "Passport copy",         15),
                new("eu_permit",    "EU residence permit",   15),
            ],
            MinFunds: null, MinSalary: null),

        [PermitCategory.DependentFamily] = new("Dependent Family Member",
            Fields:
            [
                new("FullName",            "Full name",           5),
                new("Nationality",         "Nationality",         5),
                new("Email",               "Email",               5),
                new("SponsorName",         "Sponsor name",        15),
                new("SponsorPermitNumber", "Sponsor permit number", 15),
                new("PassportExpiry",      "Passport expiry",     10),
            ],
            Docs:
            [
                new("dependency", "Dependency proof",       20),
                new("passport",   "Passport copy",          15),
                new("sponsor",    "Sponsor's permit copy",  15),
            ],
            MinFunds: null, MinSalary: null),
    };

    public static PermitRule For(PermitCategory cat) =>
        Rules.TryGetValue(cat, out var r) ? r : Rules[PermitCategory.EmployeePermit];

    // Kept for backward compat
    public static PermitRule For(PermitType type) =>
        type == PermitType.Student ? For(PermitCategory.StudentHigherEd) : For(PermitCategory.EmployeePermit);

    public const int ScoreGreen  = 90;
    public const int ScoreYellow = 70;
    public const int RiskLow     = 25;
    public const int RiskHigh    = 60;
    public const int PassportMinDays = 180;
}
