namespace PermitReady;

public static class ScoringService
{
    private static readonly string[] SuspiciousKeywords = ["test", "fake", "sample", "dummy", "xxx", "123"];
    private static readonly string[] GenericEmployers    = ["company", "employer", "firm", "inc", "ltd"];

    public static ScreeningResult Score(ApplicationInput input)
    {
        var rule = PermitRules.For(input.Category);
        var missing = new List<string>();
        var risks   = new List<string>();

        int completeness = Completeness(input, rule, missing);
        int risk         = Risk(input, rule, risks);

        var routing = completeness >= PermitRules.ScoreGreen  && risk <= PermitRules.RiskLow
            ? RoutingType.FastTrack
            : completeness >= PermitRules.ScoreYellow && risk <= PermitRules.RiskHigh
                ? RoutingType.SupplementRequested
                : RoutingType.SpecialistReview;

        return new ScreeningResult(completeness, risk, routing, missing, risks);
    }

    private static int Completeness(ApplicationInput input, PermitRule rule, List<string> missing)
    {
        int score = 0;

        foreach (var field in rule.Fields)
        {
            if (HasField(input, field.FieldName))
                score += field.Points;
            else
                missing.Add(field.Label);
        }

        foreach (var doc in rule.Docs)
        {
            if (HasDoc(input.UploadedDocuments, doc.Keyword))
                score += doc.Points;
            else
                missing.Add(doc.Label);
        }

        if (rule.MinFunds.HasValue && input.FundsAmount.HasValue && input.FundsAmount < rule.MinFunds)
        {
            score -= 15;
            missing.Add($"Sufficient funds (min €{rule.MinFunds:N0}/month)");
        }
        if (rule.MinSalary.HasValue && input.SalaryAmount.HasValue && input.SalaryAmount < rule.MinSalary)
        {
            score -= 15;
            missing.Add($"Sufficient salary (min €{rule.MinSalary:N0}/month)");
        }

        if (input.PassportExpiry.HasValue)
        {
            var daysLeft = (input.PassportExpiry.Value - DateTime.UtcNow).Days;
            if (daysLeft < PermitRules.PassportMinDays)
            {
                score -= 20;
                missing.Add($"Passport validity ({daysLeft} days, min {PermitRules.PassportMinDays} required)");
            }
        }

        return Math.Clamp(score, 0, 100);
    }

    private static int Risk(ApplicationInput input, PermitRule rule, List<string> risks)
    {
        int score = 0;

        foreach (var doc in input.UploadedDocuments)
        {
            var lower = doc.ToLowerInvariant();
            if (SuspiciousKeywords.Any(kw => lower.Contains(kw)))
            {
                score += 25;
                risks.Add($"Suspicious document filename: {doc}");
                break;
            }
        }

        if (!HasDoc(input.UploadedDocuments, "passport"))
        {
            score += 20;
            risks.Add("Passport copy not provided");
        }

        if (input.PassportExpiry.HasValue)
        {
            var daysLeft = (input.PassportExpiry.Value - DateTime.UtcNow).Days;
            if (daysLeft < 90)
            {
                score += 30;
                risks.Add($"Passport expires very soon ({daysLeft} days)");
            }
        }

        if (rule.MinFunds.HasValue && input.FundsAmount.HasValue && input.FundsAmount < rule.MinFunds * 0.5m)
        {
            score += 20;
            risks.Add($"Funds critically low (€{input.FundsAmount:N0}, <50% of min €{rule.MinFunds:N0})");
        }
        if (rule.MinSalary.HasValue && input.SalaryAmount.HasValue && input.SalaryAmount < rule.MinSalary * 0.5m)
        {
            score += 20;
            risks.Add($"Salary critically low (€{input.SalaryAmount:N0}, <50% of min €{rule.MinSalary:N0})");
        }

        if (!string.IsNullOrEmpty(input.EmployerName))
        {
            var lower = input.EmployerName.ToLowerInvariant();
            if (GenericEmployers.Any(g => lower == g || lower.StartsWith(g + " ")))
            {
                score += 15;
                risks.Add($"Generic employer name: \"{input.EmployerName}\"");
            }
        }

        if (!string.IsNullOrEmpty(input.FullName))
        {
            var lower = input.FullName.ToLowerInvariant();
            if (SuspiciousKeywords.Any(kw => lower.Contains(kw)))
            {
                score += 25;
                risks.Add($"Suspicious applicant name: \"{input.FullName}\"");
            }
        }

        // Family-specific risk: missing sponsor info
        if (input.Group == PermitGroup.Family && string.IsNullOrWhiteSpace(input.SponsorName))
        {
            score += 15;
            risks.Add("Sponsor information missing for family permit");
        }

        // Cross-field validation using AI-extracted document data
        if (input.ExtractedDocFields is { Count: > 0 } extracted)
            score += CrossValidate(input, extracted, risks);

        return Math.Clamp(score, 0, 100);
    }

    // ── Cross-field validation using extracted document data ──────────────
    private static int CrossValidate(ApplicationInput input, Dictionary<string, string> extracted, List<string> risks)
    {
        int score = 0;

        // ── Bank balance vs declared financial figure ─────────────────────
        if (extracted.TryGetValue("bank_averagemonthlybalance", out var balStr) &&
            decimal.TryParse(balStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var bankBalance) &&
            bankBalance >= 0)
        {
            // Study / trainee: declared monthly funds must be backed by bank balance
            if (input.Group == PermitGroup.Study && input.FundsAmount is > 0)
            {
                var ratio = bankBalance / input.FundsAmount.Value;
                if (ratio < 0.5m)
                {
                    score += 25;
                    risks.Add($"Critical financial mismatch: bank statement shows avg €{bankBalance:N0}/mo but declared monthly funds are €{input.FundsAmount:N0}/mo ({(int)((1 - ratio) * 100)}% shortfall). Possible misrepresentation.");
                }
                else if (ratio < 0.8m)
                {
                    score += 10;
                    risks.Add($"Bank balance (avg €{bankBalance:N0}/mo) is below declared monthly funds (€{input.FundsAmount:N0}/mo). Request updated statement or clarification.");
                }
            }

            // Self-employed: claimed monthly income should be reflected in account activity
            if (input.Category == PermitCategory.SelfEmployed && input.SalaryAmount is > 0)
            {
                var ratio = bankBalance / input.SalaryAmount.Value;
                if (ratio < 0.3m)
                {
                    score += 20;
                    risks.Add($"Self-employed income mismatch: bank avg €{bankBalance:N0}/mo is significantly below declared income €{input.SalaryAmount:N0}/mo. Claimed income appears unsubstantiated.");
                }
                else if (ratio < 0.6m)
                {
                    score += 10;
                    risks.Add($"Self-employed: bank avg €{bankBalance:N0}/mo is moderately below declared income €{input.SalaryAmount:N0}/mo. Supporting business financials recommended.");
                }
            }

            // Working holiday: declared savings must match bank balance directly
            if (input.Category == PermitCategory.WorkingHoliday && input.FundsAmount is > 0)
            {
                if (bankBalance < input.FundsAmount.Value * 0.7m)
                {
                    score += 20;
                    risks.Add($"Working holiday: declared available savings €{input.FundsAmount:N0} not supported by bank evidence (avg €{bankBalance:N0}/mo). Significant shortfall.");
                }
            }

            // Work permits (employee): unusually low savings despite stable employment claim
            if (input.Group == PermitGroup.Work &&
                input.Category is not PermitCategory.SelfEmployed and not PermitCategory.WorkingHoliday &&
                input.SalaryAmount is > 0 && bankBalance < 200m)
            {
                score += 10;
                risks.Add($"Work permit: unusually low bank balance (avg €{bankBalance:N0}/mo) for applicant claiming €{input.SalaryAmount:N0}/mo employment. Financial stability concern.");
            }
        }

        // ── Passport holder name vs applicant name ────────────────────────
        if (extracted.TryGetValue("passport_holdername", out var docName) &&
            !string.IsNullOrWhiteSpace(docName) &&
            !string.IsNullOrWhiteSpace(input.FullName))
        {
            if (!NamesMatch(input.FullName, docName))
            {
                score += 20;
                risks.Add($"Name mismatch: application says \"{input.FullName}\" but passport shows \"{docName}\". Verify identity — could indicate fraud or name change.");
            }
        }

        // ── Passport nationality vs declared nationality ───────────────────
        if (extracted.TryGetValue("passport_nationality", out var docNat) &&
            !string.IsNullOrWhiteSpace(docNat) &&
            !string.IsNullOrWhiteSpace(input.Nationality))
        {
            if (!NationalitiesMatch(input.Nationality, docNat))
            {
                score += 15;
                risks.Add($"Nationality mismatch: declared \"{input.Nationality}\" but passport nationality is \"{docNat}\". Clarification required.");
            }
        }

        return score;
    }

    // Lenient name comparison: all tokens in the shorter name must appear in the longer
    private static bool NamesMatch(string a, string b)
    {
        static string[] Tokens(string s) => s.ToUpperInvariant()
            .Replace("-", " ").Replace("'", "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var ta = Tokens(a);
        var tb = Tokens(b);
        var (shorter, longer) = ta.Length <= tb.Length ? (ta, tb) : (tb, ta);
        return shorter.All(t => longer.Any(l => l.StartsWith(t) || t.StartsWith(l)));
    }

    // Nationality comparison tolerates abbreviations and different wordings (e.g. "Indian" vs "India")
    private static bool NationalitiesMatch(string a, string b)
    {
        a = a.ToUpperInvariant().Trim();
        b = b.ToUpperInvariant().Trim();
        if (a == b) return true;
        // Allow one containing the other (e.g. "INDIAN" contains "INDIA")
        if (a.Contains(b) || b.Contains(a)) return true;
        // Match on first 4 chars (most country roots are distinct at 4 chars)
        return a.Length >= 4 && b.Length >= 4 && a[..4] == b[..4];
    }

    private static bool HasField(ApplicationInput input, string fieldName) => fieldName switch
    {
        "FullName"              => !string.IsNullOrWhiteSpace(input.FullName),
        "Nationality"           => !string.IsNullOrWhiteSpace(input.Nationality),
        "Email"                 => !string.IsNullOrWhiteSpace(input.Email),
        "UniversityName"        => !string.IsNullOrWhiteSpace(input.UniversityName),
        "ProgramName"           => !string.IsNullOrWhiteSpace(input.ProgramName),
        "FundsAmount"           => input.FundsAmount.HasValue && input.FundsAmount > 0,
        "EmployerName"          => !string.IsNullOrWhiteSpace(input.EmployerName),
        "JobTitle"              => !string.IsNullOrWhiteSpace(input.JobTitle),
        "SalaryAmount"          => input.SalaryAmount.HasValue && input.SalaryAmount > 0,
        "EmploymentContractRef" => !string.IsNullOrWhiteSpace(input.EmploymentContractRef),
        "SponsorName"           => !string.IsNullOrWhiteSpace(input.SponsorName),
        "SponsorPermitNumber"   => !string.IsNullOrWhiteSpace(input.SponsorPermitNumber),
        "PassportExpiry"        => input.PassportExpiry.HasValue,
        _                       => false
    };

    private static bool HasDoc(List<string> docs, string keyword) =>
        docs.Any(d => d.ToLowerInvariant().Contains(keyword));
}
