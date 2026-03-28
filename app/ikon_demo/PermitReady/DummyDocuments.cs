namespace PermitReady;

// ── Document field model (for the in-app preview) ─────────────────────────

public record DocField(string Label, string Value);

public record DocPreview(
    string DocType,           // human-readable type
    string IssuerName,        // institution / authority name
    string IssuerCountry,
    string DocDate,           // issue date
    string DocNumber,         // reference / doc number
    List<DocField> Fields,    // key-value pairs to show
    string FooterNote         // e.g. "Certified copy — original held on file"
);

// ── Document content factory ──────────────────────────────────────────────

public static class DummyDocumentFactory
{
    private static readonly Random Rng = new(42); // deterministic for demo

    /// <summary>Returns preview content for a document given its filename and application context.</summary>
    public static DocPreview GetPreview(string filename, ApplicationInput input)
    {
        var lower = filename.ToLowerInvariant();
        var today = DateTime.UtcNow;
        var refNo = $"FI-{today.Year}-{Math.Abs(filename.GetHashCode()) % 90000 + 10000}";

        if (lower.Contains("passport"))        return BuildPassport(input, refNo);
        if (lower.Contains("marriage"))        return BuildMarriageCert(input, refNo);
        if (lower.Contains("birth"))           return BuildBirthCert(input, refNo);
        if (lower.Contains("employment") || lower.Contains("contract"))
                                                return BuildEmploymentContract(input, refNo);
        if (lower.Contains("degree") || lower.Contains("diploma"))
                                                return BuildDegree(input, refNo);
        if (lower.Contains("transcript"))      return BuildTranscript(input, refNo);
        if (lower.Contains("acceptance") || lower.Contains("offer"))
                                                return BuildAcceptanceLetter(input, refNo);
        if (lower.Contains("bank") || lower.Contains("statement"))
                                                return BuildBankStatement(input, refNo);
        if (lower.Contains("hosting") || lower.Contains("research"))
                                                return BuildHostingAgreement(input, refNo);
        if (lower.Contains("salary") || lower.Contains("payslip"))
                                                return BuildSalaryProof(input, refNo);
        if (lower.Contains("insurance"))       return BuildTravelInsurance(input, refNo);
        if (lower.Contains("business"))        return BuildBusinessPlan(input, refNo);
        if (lower.Contains("training") || lower.Contains("intern"))
                                                return BuildTrainingAgreement(input, refNo);
        if (lower.Contains("custody"))         return BuildCustodyDoc(input, refNo);
        if (lower.Contains("sponsor_permit") || (lower.Contains("sponsor") && lower.Contains("permit")))
                                                return BuildSponsorPermit(input, refNo);
        if (lower.Contains("exchange"))        return BuildExchangeAgreement(input, refNo);
        if (lower.Contains("assignment"))      return BuildAssignmentLetter(input, refNo);
        if (lower.Contains("accommodation"))   return BuildAccommodation(input, refNo);
        if (lower.Contains("funding") || lower.Contains("funding_letter"))
                                                return BuildFundingLetter(input, refNo);

        return BuildGenericDoc(filename, input, refNo);
    }

    // ── Document builders ─────────────────────────────────────────────────

    private static DocPreview BuildPassport(ApplicationInput input, string refNo) => new(
        DocType: "TRAVEL DOCUMENT — PASSPORT",
        IssuerName: $"Ministry of Interior — {input.Nationality} Passport Agency",
        IssuerCountry: input.Nationality,
        DocDate: DateTime.UtcNow.AddYears(-3).ToString("dd MMM yyyy"),
        DocNumber: $"P{Math.Abs(input.FullName.GetHashCode()) % 9000000 + 1000000}",
        Fields:
        [
            new("Surname",            input.FullName.Split(' ').LastOrDefault() ?? input.FullName),
            new("Given Names",        string.Join(" ", input.FullName.Split(' ').SkipLast(1))),
            new("Nationality",        input.Nationality),
            new("Date of Birth",      "15 Jun 1990"),
            new("Place of Birth",     GetCapital(input.Nationality)),
            new("Sex",                "M"),
            new("Date of Issue",      DateTime.UtcNow.AddYears(-3).ToString("dd MMM yyyy")),
            new("Date of Expiry",     input.PassportExpiry?.ToString("dd MMM yyyy") ?? "N/A"),
            new("Personal No.",       $"{Math.Abs(input.Email.GetHashCode()) % 900000000 + 100000000}"),
            new("MRZ Line 1",         $"P<{input.Nationality.ToUpperInvariant()[..3]}{input.FullName.Replace(" ","<").ToUpperInvariant(),-39}"),
            new("MRZ Line 2",         $"{Math.Abs(input.FullName.GetHashCode()) % 9000000 + 1000000}9{input.Nationality.ToUpperInvariant()[..3]}9006153M{(input.PassportExpiry?.ToString("yyMMdd") ?? "991231")}<<<<<<0"),
        ],
        FooterNote: "This passport was issued by the competent authority and is valid for all countries unless otherwise indicated."
    );

    private static DocPreview BuildMarriageCert(ApplicationInput input, string refNo) => new(
        DocType: "CERTIFICATE OF MARRIAGE",
        IssuerName: "Local Register Office / Civil Registry",
        IssuerCountry: input.Nationality,
        DocDate: DateTime.UtcNow.AddYears(-2).ToString("dd MMM yyyy"),
        DocNumber: $"MC-{refNo}",
        Fields:
        [
            new("Spouse 1 — Full Name",  input.FullName),
            new("Spouse 1 — Nationality", input.Nationality),
            new("Spouse 2 — Full Name",  input.SponsorName ?? "Mikko Aleksi Mäkinen"),
            new("Spouse 2 — Nationality", "Finnish"),
            new("Date of Marriage",      DateTime.UtcNow.AddYears(-2).ToString("dd MMMM yyyy")),
            new("Place of Marriage",     GetCapital(input.Nationality)),
            new("Registration No.",      $"REG-{refNo}"),
            new("Officiating Authority", "District Court Civil Registry"),
        ],
        FooterNote: "Certified true copy of the entry in the Marriage Register. Apostille attached on reverse."
    );

    private static DocPreview BuildBirthCert(ApplicationInput input, string refNo) => new(
        DocType: "CERTIFICATE OF BIRTH",
        IssuerName: "National Civil Registration Authority",
        IssuerCountry: input.Nationality,
        DocDate: "15 Jun 1990",
        DocNumber: $"BC-{refNo}",
        Fields:
        [
            new("Full Name of Child",  input.FullName),
            new("Date of Birth",       "15 Jun 1990"),
            new("Place of Birth",      GetCapital(input.Nationality)),
            new("Father's Name",       input.SponsorName ?? "Aleksi Mäkinen"),
            new("Mother's Name",       "Hana " + (input.FullName.Split(' ').LastOrDefault() ?? "")),
            new("Registration No.",    $"REG-{refNo}"),
            new("Date of Registration","20 Jun 1990"),
        ],
        FooterNote: "Certified copy issued by the National Civil Registration Authority. Apostille No. AP-2022-FI-44817."
    );

    private static DocPreview BuildEmploymentContract(ApplicationInput input, string refNo) => new(
        DocType: "EMPLOYMENT CONTRACT",
        IssuerName: input.EmployerName ?? "Employer Oy",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.AddMonths(-3).ToString("dd MMM yyyy"),
        DocNumber: input.EmploymentContractRef ?? refNo,
        Fields:
        [
            new("Employee",          input.FullName),
            new("Employer",          input.EmployerName ?? "Employer Oy"),
            new("Position / Title",  input.JobTitle ?? "Specialist"),
            new("Employment Type",   "Permanent, full-time"),
            new("Start Date",        DateTime.UtcNow.AddMonths(2).ToString("dd MMM yyyy")),
            new("Gross Salary",      input.SalaryAmount.HasValue ? $"€ {input.SalaryAmount:N2} / month" : "As agreed"),
            new("Working Hours",     "37.5 h / week"),
            new("Probation Period",  "4 months"),
            new("Collective Agreement", "Technology Industries of Finland — Employees TES"),
            new("Location",          "Helsinki, Finland"),
            new("Contract Ref.",     input.EmploymentContractRef ?? refNo),
        ],
        FooterNote: "Signed by both parties. Employer registered with Finnish Patent and Registration Office (PRH). Business ID: " + GenerateBid()
    );

    private static DocPreview BuildDegree(ApplicationInput input, string refNo) => new(
        DocType: "CERTIFICATE OF HIGHER EDUCATION",
        IssuerName: input.UniversityName ?? "European University of Technology",
        IssuerCountry: input.Nationality,
        DocDate: DateTime.UtcNow.AddYears(-4).ToString("dd MMM yyyy"),
        DocNumber: $"DEG-{refNo}",
        Fields:
        [
            new("Graduate",          input.FullName),
            new("Degree Awarded",    input.ProgramName ?? "Bachelor of Science (Technology)"),
            new("Major / Field",     "Computer Science and Engineering"),
            new("Grade",             "Distinction (GPA 3.8 / 4.0)"),
            new("Date Conferred",    DateTime.UtcNow.AddYears(-4).ToString("dd MMMM yyyy")),
            new("Duration",          "4 years (240 ECTS)"),
            new("Diploma No.",       $"DIP-{refNo}"),
            new("Accreditation",     "National Quality Assurance Agency — Reg. No. NQA/2019/0042"),
        ],
        FooterNote: "This degree is recognised under the Lisbon Recognition Convention. Certified copy — original held by registrar."
    );

    private static DocPreview BuildTranscript(ApplicationInput input, string refNo) => new(
        DocType: "OFFICIAL ACADEMIC TRANSCRIPT",
        IssuerName: input.UniversityName ?? "University of Technology",
        IssuerCountry: input.Nationality,
        DocDate: DateTime.UtcNow.AddMonths(-6).ToString("dd MMM yyyy"),
        DocNumber: $"TR-{refNo}",
        Fields:
        [
            new("Student Name",     input.FullName),
            new("Student ID",       $"STU-{Math.Abs(input.Email.GetHashCode()) % 900000 + 100000}"),
            new("Programme",        input.ProgramName ?? "Bachelor of Science"),
            new("Cumulative GPA",   "3.7 / 4.0 (Distinction)"),
            new("Credits Completed","210 / 240 ECTS"),
            new("Academic Standing","Good Standing"),
            new("Graduation Date",  "Expected June " + (DateTime.UtcNow.Year + 1)),
            new("Issued By",        "Registrar's Office"),
        ],
        FooterNote: "Official transcript — sealed and stamped. Any alterations render this document invalid."
    );

    private static DocPreview BuildAcceptanceLetter(ApplicationInput input, string refNo) => new(
        DocType: "LETTER OF ACCEPTANCE",
        IssuerName: input.UniversityName ?? "University of Helsinki",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.AddMonths(-4).ToString("dd MMM yyyy"),
        DocNumber: $"ADM-{refNo}",
        Fields:
        [
            new("Applicant",          input.FullName),
            new("Programme Offered",  input.ProgramName ?? "Master of Science"),
            new("Start Date",         "01 September " + DateTime.UtcNow.Year),
            new("Duration",           "2 years (120 ECTS)"),
            new("Tuition Fee",        "Free — EU/EEA applicant waiver"),
            new("Language",           "English"),
            new("Student Status",     "Full-time"),
            new("Conditional",        "No — unconditional offer"),
            new("Issued By",          "Admissions Office"),
            new("Reference",          $"ADM-{refNo}"),
        ],
        FooterNote: "This offer is valid for 90 days. To confirm your place, log in to the Student Portal and accept the offer."
    );

    private static DocPreview BuildBankStatement(ApplicationInput input, string refNo) => new(
        DocType: "BANK STATEMENT",
        IssuerName: "International Bank — Personal Banking Division",
        IssuerCountry: input.Nationality,
        DocDate: DateTime.UtcNow.ToString("dd MMM yyyy"),
        DocNumber: $"STMT-{refNo}",
        Fields:
        [
            new("Account Holder",   input.FullName),
            new("IBAN",             GenerateIBAN(input.Nationality)),
            new("Account Type",     "Current / Checking"),
            new("Statement Period", $"{DateTime.UtcNow.AddMonths(-3):MMM yyyy} – {DateTime.UtcNow:MMM yyyy}"),
            new("Opening Balance",  $"€ {(input.FundsAmount ?? 1500m) * 12 + 3000m:N2}"),
            new("Closing Balance",  $"€ {(input.FundsAmount ?? 1500m) * 12 + 2400m:N2}"),
            new("Average Monthly",  $"€ {input.FundsAmount ?? 1500m:N2}"),
            new("Currency",         "EUR"),
            new("Certified By",     "Head of Retail Banking Operations"),
        ],
        FooterNote: "This statement is an official document of the bank. For queries, contact your branch. Branch Ref: " + refNo
    );

    private static DocPreview BuildHostingAgreement(ApplicationInput input, string refNo) => new(
        DocType: "HOSTING AGREEMENT FOR RESEARCHER",
        IssuerName: input.EmployerName ?? "University of Helsinki",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.AddMonths(-2).ToString("dd MMM yyyy"),
        DocNumber: $"HA-{refNo}",
        Fields:
        [
            new("Researcher",            input.FullName),
            new("Host Institution",      input.EmployerName ?? "University of Helsinki"),
            new("Department",            "Department of Computer Science"),
            new("Research Project",      "Advanced AI Systems for Healthcare Informatics"),
            new("Position",              input.JobTitle ?? "Postdoctoral Researcher"),
            new("Start Date",            DateTime.UtcNow.AddMonths(2).ToString("dd MMM yyyy")),
            new("End Date",              DateTime.UtcNow.AddMonths(26).ToString("dd MMM yyyy")),
            new("Funding",               $"€ {input.SalaryAmount ?? 2500m:N0} / month (Academy of Finland Grant)"),
            new("Supervisor",            "Prof. Dr. Anna-Liisa Virtanen"),
            new("Agreement No.",         $"HA-{refNo}"),
        ],
        FooterNote: "Signed by Director of International Affairs. Research Council of Finland — Grant Ref: AKA-2024-0881."
    );

    private static DocPreview BuildSalaryProof(ApplicationInput input, string refNo) => new(
        DocType: "PAYSLIP / SALARY CERTIFICATE",
        IssuerName: input.EmployerName ?? "Employer Oy",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.ToString("dd MMM yyyy"),
        DocNumber: $"PAY-{refNo}",
        Fields:
        [
            new("Employee",          input.FullName),
            new("Employee ID",       $"EMP-{Math.Abs(input.Email.GetHashCode()) % 90000 + 10000}"),
            new("Department",        "Engineering & Technology"),
            new("Pay Period",        $"{DateTime.UtcNow:MMMM yyyy}"),
            new("Gross Salary",      $"€ {input.SalaryAmount ?? 3500m:N2}"),
            new("Income Tax (24%)",  $"€ {(input.SalaryAmount ?? 3500m) * 0.24m:N2}"),
            new("Employee Pension",  $"€ {(input.SalaryAmount ?? 3500m) * 0.0715m:N2}"),
            new("Net Pay",           $"€ {(input.SalaryAmount ?? 3500m) * 0.6885m:N2}"),
            new("Payment Method",    "Bank transfer to IBAN on file"),
            new("Employer TIN",      GenerateBid()),
        ],
        FooterNote: "This payslip is a legally valid document. Employer registered and verified with Finnish Tax Administration (Vero)."
    );

    private static DocPreview BuildTravelInsurance(ApplicationInput input, string refNo) => new(
        DocType: "TRAVEL & HEALTH INSURANCE CERTIFICATE",
        IssuerName: "GlobalCare Insurance Group",
        IssuerCountry: "Germany",
        DocDate: DateTime.UtcNow.ToString("dd MMM yyyy"),
        DocNumber: $"GCI-{refNo}",
        Fields:
        [
            new("Policyholder",      input.FullName),
            new("Policy Number",     $"GCI-{refNo}"),
            new("Coverage Type",     "Comprehensive Travel + Medical"),
            new("Coverage Territory","Schengen Area + Finland"),
            new("Valid From",        DateTime.UtcNow.AddMonths(1).ToString("dd MMM yyyy")),
            new("Valid Until",       DateTime.UtcNow.AddMonths(13).ToString("dd MMM yyyy")),
            new("Medical Cover",     "€ 30,000 per incident"),
            new("Repatriation",      "Included"),
            new("Premium Paid",      "€ 320.00 (annual)"),
        ],
        FooterNote: "Policy compliant with Schengen Visa requirements (Regulation EC No 810/2009). Emergency line: +49 800 000 0000."
    );

    private static DocPreview BuildBusinessPlan(ApplicationInput input, string refNo) => new(
        DocType: "BUSINESS PLAN SUMMARY",
        IssuerName: input.FullName + " (Self-employed)",
        IssuerCountry: "Finland (proposed)",
        DocDate: DateTime.UtcNow.ToString("dd MMM yyyy"),
        DocNumber: $"BP-{refNo}",
        Fields:
        [
            new("Applicant / Founder", input.FullName),
            new("Business Name",       (input.FullName.Split(' ').LastOrDefault() ?? "X") + " Consulting Oy"),
            new("Business Type",       input.JobTitle ?? "IT Consulting / Software Development"),
            new("Projected Revenue Y1","€ 60,000 – 80,000"),
            new("Projected Revenue Y2","€ 90,000 – 120,000"),
            new("Start Capital",       "€ 15,000 (personal savings)"),
            new("Clients (LOI)",       "3 Finnish companies — Letters of Intent enclosed"),
            new("Operating Costs Y1",  "€ 30,000 (office, tools, misc)"),
            new("Net Projected Y1",    "€ 30,000 – 50,000"),
            new("Business Advisor",    "Enterprise Finland / NewCo Helsinki"),
        ],
        FooterNote: "Business plan reviewed and certified by NewCo Helsinki. Registration pending PRH approval. Ref: NewCo-2024-FI-04812."
    );

    private static DocPreview BuildTrainingAgreement(ApplicationInput input, string refNo) => new(
        DocType: "TRAINING / INTERNSHIP AGREEMENT",
        IssuerName: input.EmployerName ?? "Reaktor Innovations Oy",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.AddMonths(-1).ToString("dd MMM yyyy"),
        DocNumber: $"TA-{refNo}",
        Fields:
        [
            new("Trainee",            input.FullName),
            new("Host Organisation",  input.EmployerName ?? "Reaktor Innovations Oy"),
            new("Supervisor",         "Dr. Tiina Korhonen"),
            new("Training Field",     input.JobTitle ?? "Software Engineering"),
            new("Start Date",         DateTime.UtcNow.AddMonths(2).ToString("dd MMM yyyy")),
            new("End Date",           DateTime.UtcNow.AddMonths(8).ToString("dd MMM yyyy")),
            new("Duration",           "6 months"),
            new("Stipend",            $"€ {input.FundsAmount ?? 600m:N0} / month"),
            new("Academic Credits",   "15 ECTS (accredited by home university)"),
            new("Agreement Ref.",     $"TA-{refNo}"),
        ],
        FooterNote: "Agreement concluded under the European Credit Transfer System (ECTS) and Finnish labour law."
    );

    private static DocPreview BuildCustodyDoc(ApplicationInput input, string refNo) => new(
        DocType: "CUSTODY AGREEMENT / COURT ORDER",
        IssuerName: "District Court of Helsinki",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.AddYears(-1).ToString("dd MMM yyyy"),
        DocNumber: $"CCO-{refNo}",
        Fields:
        [
            new("Child's Full Name",    input.FullName),
            new("Date of Birth",        "15 Jun 2018"),
            new("Parent (Applicant)",   input.SponsorName ?? "Aleksi Mäkinen"),
            new("Custody Type",         "Sole custody granted to applicant"),
            new("Finnish Nationality",  "Confirmed — Finnish citizen by birth"),
            new("Court Reference",      $"CCO-{refNo}"),
            new("Judge",                "Presiding Judge Kaisa Leinonen"),
            new("Date of Order",        DateTime.UtcNow.AddYears(-1).ToString("dd MMM yyyy")),
        ],
        FooterNote: "This court order is enforceable under Finnish law (Laki lapsen huollosta ja tapaamisoikeudesta 361/1983)."
    );

    private static DocPreview BuildSponsorPermit(ApplicationInput input, string refNo) => new(
        DocType: "RESIDENCE PERMIT — SPONSOR COPY",
        IssuerName: "Finnish Immigration Service (Migri)",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.AddYears(-1).ToString("dd MMM yyyy"),
        DocNumber: input.SponsorPermitNumber ?? $"RP-{refNo}",
        Fields:
        [
            new("Permit Holder",     input.SponsorName ?? "Sponsor Name"),
            new("Permit Type",       "Continuous Residence Permit (A)"),
            new("Valid From",        DateTime.UtcNow.AddYears(-1).ToString("dd MMM yyyy")),
            new("Valid Until",       DateTime.UtcNow.AddYears(3).ToString("dd MMM yyyy")),
            new("Issued By",         "Migri — Resident Registration Unit"),
            new("Permit Number",     input.SponsorPermitNumber ?? $"RP-{refNo}"),
            new("Municipality",      "Helsinki"),
            new("Purpose",           "Work — Permanent employment"),
        ],
        FooterNote: "This is a certified copy of the sponsor's residence permit issued by Migri. Original presented during appointment."
    );

    private static DocPreview BuildExchangeAgreement(ApplicationInput input, string refNo) => new(
        DocType: "EXCHANGE PROGRAMME AGREEMENT",
        IssuerName: input.UniversityName ?? "University of Turku",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.AddMonths(-3).ToString("dd MMM yyyy"),
        DocNumber: $"EP-{refNo}",
        Fields:
        [
            new("Student",               input.FullName),
            new("Home University",       "University of " + GetCapital(input.Nationality)),
            new("Host University",       input.UniversityName ?? "University of Turku"),
            new("Exchange Programme",    "Erasmus+ / International Exchange"),
            new("Academic Year",         $"{DateTime.UtcNow.Year}–{DateTime.UtcNow.Year + 1}"),
            new("Duration",              "One semester (5 months)"),
            new("Credits",               "30 ECTS"),
            new("Erasmus+ Grant",        "€ 540 / month (European Commission)"),
            new("Agreement Ref.",        $"EP-{refNo}"),
        ],
        FooterNote: "Agreement signed by International Relations Coordinators of both institutions under the Erasmus Charter for Higher Education."
    );

    private static DocPreview BuildAssignmentLetter(ApplicationInput input, string refNo) => new(
        DocType: "INTRA-CORPORATE TRANSFER ASSIGNMENT LETTER",
        IssuerName: input.EmployerName ?? "Global Technology Corp",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.AddMonths(-1).ToString("dd MMM yyyy"),
        DocNumber: $"ICT-{refNo}",
        Fields:
        [
            new("Employee",          input.FullName),
            new("Sending Entity",    (input.EmployerName ?? "Global Tech") + " — Head Office"),
            new("Receiving Entity",  (input.EmployerName ?? "Global Tech") + " Finland Oy"),
            new("Position",          input.JobTitle ?? "Senior Manager"),
            new("Assignment Start",  DateTime.UtcNow.AddMonths(2).ToString("dd MMM yyyy")),
            new("Assignment End",    DateTime.UtcNow.AddMonths(38).ToString("dd MMM yyyy")),
            new("Monthly Salary",    $"€ {input.SalaryAmount ?? 5000m:N2}"),
            new("Assignment Ref.",   $"ICT-{refNo}"),
            new("Authorised By",     "Chief People Officer"),
        ],
        FooterNote: "This assignment letter is issued in accordance with Finnish Aliens Act §81a (Intra-Corporate Transferees Directive 2014/66/EU)."
    );

    private static DocPreview BuildAccommodation(ApplicationInput input, string refNo) => new(
        DocType: "ACCOMMODATION CONFIRMATION",
        IssuerName: "Helsinki Housing Services",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.ToString("dd MMM yyyy"),
        DocNumber: $"ACC-{refNo}",
        Fields:
        [
            new("Tenant",             input.FullName),
            new("Property Address",   "Mannerheimintie 12 A 3, 00100 Helsinki, Finland"),
            new("Room Type",          "Furnished studio, 28 m²"),
            new("Lease Start",        DateTime.UtcNow.AddMonths(2).ToString("dd MMM yyyy")),
            new("Lease End",          DateTime.UtcNow.AddMonths(14).ToString("dd MMM yyyy")),
            new("Monthly Rent",       "€ 750 (utilities included)"),
            new("Deposit Paid",       "€ 1,500"),
            new("Landlord",           "Helsinki Housing Services Oy"),
            new("Contract Ref.",      $"ACC-{refNo}"),
        ],
        FooterNote: "Registered lease agreement. Finnish law applies. Tenancy Act (Laki asuinhuoneiston vuokrauksesta 481/1995)."
    );

    private static DocPreview BuildFundingLetter(ApplicationInput input, string refNo) => new(
        DocType: "RESEARCH FUNDING LETTER",
        IssuerName: "Academy of Finland / Research Council",
        IssuerCountry: "Finland",
        DocDate: DateTime.UtcNow.AddMonths(-2).ToString("dd MMM yyyy"),
        DocNumber: $"AKA-{refNo}",
        Fields:
        [
            new("Grant Holder",      input.FullName),
            new("Funding Body",      "Academy of Finland — Research Council for Natural Sciences"),
            new("Grant Title",       "Novel AI Systems for Predictive Healthcare"),
            new("Grant Amount",      $"€ {(input.SalaryAmount ?? 2500m) * 24m:N0} (2-year project)"),
            new("Monthly Salary",    $"€ {input.SalaryAmount ?? 2500m:N0} / month"),
            new("Grant Period",      $"01 {DateTime.UtcNow.AddMonths(2):MMM yyyy} – 28 Feb {DateTime.UtcNow.Year + 2}"),
            new("Host Institution",  input.EmployerName ?? "University of Helsinki"),
            new("Grant Reference",   $"AKA-{refNo}"),
        ],
        FooterNote: "Funding decision issued under Academy of Finland Act (922/2009). Grant disbursed monthly to host institution."
    );

    private static DocPreview BuildGenericDoc(string filename, ApplicationInput input, string refNo) => new(
        DocType: "SUPPORTING DOCUMENT",
        IssuerName: "Issuing Authority",
        IssuerCountry: input.Nationality,
        DocDate: DateTime.UtcNow.AddMonths(-1).ToString("dd MMM yyyy"),
        DocNumber: refNo,
        Fields:
        [
            new("Subject",       input.FullName),
            new("Document Name", filename),
            new("Reference",     refNo),
            new("Category",      input.Category.DisplayName()),
        ],
        FooterNote: "This document was submitted as part of the residence permit application."
    );

    // ── Helpers ──────────────────────────────────────────────────────────

    private static string GetCapital(string nationality) => nationality switch
    {
        "Syrian" or "Syria"         => "Damascus",
        "Indian" or "India"         => "New Delhi",
        "Russian" or "Russia"       => "Moscow",
        "Japanese" or "Japan"       => "Tokyo",
        "Moroccan" or "Morocco"     => "Rabat",
        "Brazilian" or "Brazil"     => "Brasília",
        "Chinese" or "China"        => "Beijing",
        "Ghanaian" or "Ghana"       => "Accra",
        "Nigerian" or "Nigeria"     => "Abuja",
        "Spanish" or "Spain"        => "Madrid",
        "Ukrainian" or "Ukraine"    => "Kyiv",
        "Pakistani" or "Pakistan"   => "Islamabad",
        "Guinean" or "Guinea"       => "Conakry",
        "Finnish" or "Finland"      => "Helsinki",
        _                           => nationality + " (Capital)"
    };

    private static string GenerateIBAN(string nationality) => nationality switch
    {
        "Finnish" => $"FI{Rng.Next(10,99)} {Rng.Next(1000,9999)} {Rng.Next(1000,9999)} {Rng.Next(1000,9999)} {Rng.Next(10,99)}",
        _         => $"DE{Rng.Next(10,99)} {Rng.Next(1000,9999)} {Rng.Next(1000,9999)} {Rng.Next(1000,9999)} {Rng.Next(1000,9999)} {Rng.Next(10,99)}"
    };

    private static string GenerateBid() =>
        $"{Rng.Next(1000000,9999999)}-{Rng.Next(1,9)}";

    // ── Minimal PDF generator ─────────────────────────────────────────────

    /// <summary>Generates a valid minimal PDF byte array from a DocPreview.</summary>
    public static byte[] GeneratePdf(DocPreview doc)
    {
        var lines = new System.Text.StringBuilder();

        // Build the visible text content
        lines.AppendLine($"  {doc.DocType}");
        lines.AppendLine();
        lines.AppendLine($"  Issued by: {doc.IssuerName}");
        lines.AppendLine($"  Country:   {doc.IssuerCountry}");
        lines.AppendLine($"  Date:      {doc.DocDate}");
        lines.AppendLine($"  Ref. No.:  {doc.DocNumber}");
        lines.AppendLine();
        lines.AppendLine("  ─────────────────────────────────────────────────────────");
        lines.AppendLine();

        foreach (var f in doc.Fields)
            lines.AppendLine($"  {f.Label,-30} {f.Value}");

        lines.AppendLine();
        lines.AppendLine("  ─────────────────────────────────────────────────────────");
        lines.AppendLine();
        lines.AppendLine($"  {doc.FooterNote}");
        lines.AppendLine();
        lines.AppendLine("  [OFFICIAL STAMP]                    [AUTHORISED SIGNATURE]");
        lines.AppendLine();
        lines.AppendLine("  PermitReady Demo Document — Not a real official document.");

        return BuildMinimalPdf(doc.DocType, lines.ToString());
    }

    private static byte[] BuildMinimalPdf(string title, string body)
    {
        // Escape special PDF string characters
        string Esc(string s) => s
            .Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")
            .Replace("\r", "").Replace("€", "EUR");

        // Split into lines, trim to fit page width (~90 chars)
        var textLines = body.Split('\n')
            .Select(l => l.Length > 95 ? l[..95] : l)
            .ToList();

        // Build BT…ET block
        var btBlock = new System.Text.StringBuilder();
        btBlock.Append("BT\n/F1 10 Tf\n50 780 Td\n14 TL\n");
        // Title in larger font
        btBlock.Append($"14 Tf ({Esc(title)}) Tj\n10 Tf\n");
        foreach (var line in textLines)
            btBlock.Append($"T* ({Esc(line)}) Tj\n");
        btBlock.Append("ET\n");

        var stream = System.Text.Encoding.Latin1.GetBytes(btBlock.ToString());

        var objects = new List<string>
        {
            // obj 1: catalog
            "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
            // obj 2: pages
            "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n",
            // obj 3: page
            "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 842]\n" +
            "   /Contents 4 0 R\n   /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n",
            // obj 4: content stream
            $"4 0 obj\n<< /Length {stream.Length} >>\nstream\n" + btBlock + "endstream\nendobj\n",
            // obj 5: font
            "5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>\nendobj\n",
        };

        var pdf = new System.Text.StringBuilder();
        pdf.Append("%PDF-1.4\n");

        var offsets = new int[objects.Count + 1];
        for (int i = 0; i < objects.Count; i++)
        {
            offsets[i] = pdf.Length;
            pdf.Append(objects[i]);
        }

        int xrefOffset = pdf.Length;
        offsets[objects.Count] = xrefOffset;

        pdf.Append("xref\n");
        pdf.Append($"0 {objects.Count + 1}\n");
        pdf.Append("0000000000 65535 f \n");
        for (int i = 0; i < objects.Count; i++)
            pdf.Append($"{offsets[i]:D10} 00000 n \n");

        pdf.Append("trailer\n");
        pdf.Append($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
        pdf.Append("startxref\n");
        pdf.Append($"{xrefOffset}\n");
        pdf.Append("%%EOF\n");

        return System.Text.Encoding.Latin1.GetBytes(pdf.ToString());
    }
}
