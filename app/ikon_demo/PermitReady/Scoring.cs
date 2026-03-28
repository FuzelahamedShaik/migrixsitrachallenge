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

        return Math.Clamp(score, 0, 100);
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
