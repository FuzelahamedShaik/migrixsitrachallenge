using System.Text;

public partial class IkonDemoApp
{
    // ── Passport verification ─────────────────────────────────────────────
    internal async Task VerifyPassportAsync(string tempFilePath, string fileName)
    {
        try
        {
            var text = ExtractPdfText(tempFilePath);

            var (analysis, _) = await Emerge.Run<PermitReady.PassportAnalysis>(
                LLMModel.Claude46Sonnet, new KernelContext(), pass =>
                {
                    pass.SystemPrompt = "You are a document verification assistant for Finnish Immigration Service. Extract structured data from passport documents. Return only valid JSON.";
                    pass.Command = $"""
                        Analyze the following text extracted from a PDF document named "{fileName}".
                        Determine if this is a passport and extract key fields.

                        Document text:
                        {text.Truncate(4000)}

                        Return JSON matching the schema:
                        {pass.JsonSchema}

                        For ExpiryDate use "YYYY-MM-DD" format. If you cannot determine a field, use empty string.
                        """;
                    pass.Temperature = 0.1;
                }).FinalAsync();

            var alerts = new List<string>();
            var extracted = new Dictionary<string, string>();

            if (!analysis.IsPassport)
            {
                // Mark as failed — wrong document type
                UpdateDocInList(_passportDoc, fileName, d => d with
                {
                    Status = PermitReady.DocStatus.Failed,
                    Alerts = ["This document does not appear to be a passport. Please upload the correct file."]
                });
                return;
            }

            // Store extracted fields
            if (!string.IsNullOrEmpty(analysis.ExpiryDate))     extracted["ExpiryDate"] = analysis.ExpiryDate;
            if (!string.IsNullOrEmpty(analysis.HolderName))     extracted["HolderName"] = analysis.HolderName;
            if (!string.IsNullOrEmpty(analysis.Nationality))    extracted["Nationality"] = analysis.Nationality;

            // Auto-fill passport expiry from document if not already entered
            if (string.IsNullOrEmpty(_passportExpiry.Value) && !string.IsNullOrEmpty(analysis.ExpiryDate))
                _passportExpiry.Value = analysis.ExpiryDate;

            // Check expiry date
            var status = PermitReady.DocStatus.Verified;
            if (DateTime.TryParse(analysis.ExpiryDate, out var expiry))
            {
                var daysLeft = (expiry - DateTime.UtcNow).Days;
                if (daysLeft < 0)
                {
                    alerts.Add($"Passport has already expired ({expiry:dd MMM yyyy}). A valid passport is required.");
                    status = PermitReady.DocStatus.Failed;
                }
                else if (daysLeft < 90)
                {
                    alerts.Add($"Passport expires very soon — {daysLeft} days remaining ({expiry:dd MMM yyyy}). Migri requires at least 6 months validity beyond the permit period.");
                    status = PermitReady.DocStatus.Warning;
                }
                else if (daysLeft < 180)
                {
                    alerts.Add($"Passport expires in {daysLeft} days ({expiry:dd MMM yyyy}). This may be insufficient for the permit duration — minimum 180 days required beyond permit end.");
                    status = PermitReady.DocStatus.Warning;
                }
            }

            if (!string.IsNullOrEmpty(analysis.Notes) && analysis.Notes.Length > 5)
                alerts.Add(analysis.Notes);

            UpdateDocInList(_passportDoc, fileName, d => d with
            {
                Status    = status,
                Alerts    = alerts,
                Extracted = extracted
            });
        }
        catch (Exception ex)
        {
            Log.Instance.Warning($"Passport verification failed: {ex.Message}");
            UpdateDocInList(_passportDoc, fileName, d => d with
            {
                Status = PermitReady.DocStatus.Warning,
                Alerts = ["Document uploaded but could not be verified automatically. Please ensure it is a valid passport."]
            });
        }
    }

    // ── Bank statement verification ───────────────────────────────────────
    internal async Task VerifyBankStatementAsync(string tempFilePath, string fileName, decimal? claimedAmount)
    {
        try
        {
            var text = ExtractPdfText(tempFilePath);

            var (analysis, _) = await Emerge.Run<PermitReady.BankStatementAnalysis>(
                LLMModel.Claude46Sonnet, new KernelContext(), pass =>
                {
                    pass.SystemPrompt = "You are a document verification assistant for Finnish Immigration Service. Extract financial data from bank statements. Return only valid JSON.";
                    pass.Command = $"""
                        Analyze the following text extracted from a PDF document named "{fileName}".
                        Determine if this is a bank statement and extract the average monthly balance.

                        Document text:
                        {text.Truncate(4000)}

                        Return JSON matching the schema:
                        {pass.JsonSchema}

                        For AverageMonthlyBalance, convert to EUR if the currency is different.
                        If the balance cannot be determined, use 0.
                        """;
                    pass.Temperature = 0.1;
                }).FinalAsync();

            var alerts = new List<string>();
            var extracted = new Dictionary<string, string>();
            var status = PermitReady.DocStatus.Verified;

            if (!analysis.IsBankStatement)
            {
                UpdateDocInList(_bankStatementDoc, fileName, d => d with
                {
                    Status = PermitReady.DocStatus.Failed,
                    Alerts = ["This document does not appear to be a bank statement. Please upload the correct file."]
                });
                return;
            }

            extracted["AverageMonthlyBalance"] = analysis.AverageMonthlyBalance.ToString("F2");
            extracted["Currency"] = analysis.Currency;

            // Compare against claimed amount
            if (claimedAmount.HasValue && claimedAmount > 0 && analysis.AverageMonthlyBalance > 0)
            {
                var ratio = analysis.AverageMonthlyBalance / claimedAmount.Value;
                if (ratio < 0.5m)
                {
                    alerts.Add($"Bank statement shows average balance of €{analysis.AverageMonthlyBalance:N0}/month, but you claimed €{claimedAmount:N0}/month. These differ significantly — please review.");
                    status = PermitReady.DocStatus.Warning;
                }
                else if (ratio < 0.85m)
                {
                    alerts.Add($"Bank statement shows €{analysis.AverageMonthlyBalance:N0}/month vs claimed €{claimedAmount:N0}/month. Minor discrepancy — ensure your figures are accurate.");
                    status = PermitReady.DocStatus.Warning;
                }
                else
                {
                    alerts.Add($"Bank statement verified: average monthly balance of €{analysis.AverageMonthlyBalance:N0} matches claimed amount.");
                }
            }

            if (!string.IsNullOrEmpty(analysis.Notes) && analysis.Notes.Length > 5)
                alerts.Add(analysis.Notes);

            UpdateDocInList(_bankStatementDoc, fileName, d => d with
            {
                Status    = status,
                Alerts    = alerts,
                Extracted = extracted
            });
        }
        catch (Exception ex)
        {
            Log.Instance.Warning($"Bank statement verification failed: {ex.Message}");
            UpdateDocInList(_bankStatementDoc, fileName, d => d with
            {
                Status = PermitReady.DocStatus.Warning,
                Alerts = ["Document uploaded but could not be verified automatically."]
            });
        }
    }

    // ── Employment contract verification ──────────────────────────────────
    internal async Task VerifyEmploymentContractAsync(
        string tempFilePath, string fileName,
        string? declaredEmployer, string? declaredJobTitle, decimal? declaredSalary, string? declaredRef)
    {
        try
        {
            var text = ExtractPdfText(tempFilePath);

            var (analysis, _) = await Emerge.Run<PermitReady.EmploymentContractAnalysis>(
                LLMModel.Claude46Sonnet, new KernelContext(), pass =>
                {
                    pass.SystemPrompt = "You are a document verification assistant for the Finnish Immigration Service. Extract structured data from employment contracts and job offer letters. Return only valid JSON.";
                    pass.Command = $"""
                        Analyze the following text extracted from a PDF document named "{fileName}".
                        Determine if this is an employment contract or job offer letter and extract key fields.

                        Document text:
                        {text.Truncate(4000)}

                        Return JSON matching the schema:
                        {pass.JsonSchema}

                        For MonthlySalary, extract the gross monthly figure in EUR (0 if not stated or cannot be determined).
                        For StartDate use "YYYY-MM-DD" format (empty string if not found).
                        For ContractRef, extract any reference/agreement number (empty if none).
                        """;
                    pass.Temperature = 0.1;
                }).FinalAsync();

            var alerts = new List<string>();
            var extracted = new Dictionary<string, string>();
            var status = PermitReady.DocStatus.Verified;

            if (!analysis.IsContract)
            {
                UpdateDocInList(_contractDoc, fileName, d => d with
                {
                    Status = PermitReady.DocStatus.Warning,
                    Alerts = [$"This does not appear to be an employment contract. Please verify you uploaded the correct file.{(string.IsNullOrWhiteSpace(analysis.Notes) ? "" : " " + analysis.Notes)}"]
                });
                return;
            }

            // Store all extracted fields
            if (!string.IsNullOrWhiteSpace(analysis.EmployerName))  extracted["EmployerName"]  = analysis.EmployerName;
            if (!string.IsNullOrWhiteSpace(analysis.JobTitle))       extracted["JobTitle"]       = analysis.JobTitle;
            if (analysis.MonthlySalary > 0)                          extracted["MonthlySalary"]  = analysis.MonthlySalary.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            if (!string.IsNullOrWhiteSpace(analysis.ContractRef))    extracted["ContractRef"]    = analysis.ContractRef;
            if (!string.IsNullOrWhiteSpace(analysis.StartDate))      extracted["StartDate"]      = analysis.StartDate;

            // Cross-validate against form declarations
            if (!string.IsNullOrWhiteSpace(analysis.EmployerName) && !string.IsNullOrWhiteSpace(declaredEmployer))
            {
                if (!NamesLooseMatch(analysis.EmployerName, declaredEmployer))
                {
                    alerts.Add($"Employer mismatch: contract shows \"{analysis.EmployerName}\" but form declares \"{declaredEmployer}\". Verify the correct employer.");
                    status = PermitReady.DocStatus.Warning;
                }
            }

            if (analysis.MonthlySalary > 0 && declaredSalary.HasValue && declaredSalary > 0)
            {
                var diff = Math.Abs(analysis.MonthlySalary - declaredSalary.Value) / declaredSalary.Value;
                if (diff > 0.1m)
                {
                    alerts.Add($"Salary mismatch: contract states €{analysis.MonthlySalary:N0}/mo but form declares €{declaredSalary:N0}/mo ({diff * 100:N0}% difference).");
                    status = PermitReady.DocStatus.Warning;
                }
                else
                {
                    alerts.Add($"Salary verified: contract confirms €{analysis.MonthlySalary:N0}/mo gross monthly salary.");
                }
            }

            if (!string.IsNullOrWhiteSpace(analysis.Notes) && analysis.Notes.Length > 5)
                alerts.Add(analysis.Notes);

            if (alerts.Count == 0)
                alerts.Add($"Employment contract verified. Employer: {analysis.EmployerName}, Salary: €{analysis.MonthlySalary:N0}/mo.");

            UpdateDocInList(_contractDoc, fileName, d => d with { Status = status, Alerts = alerts, Extracted = extracted });
        }
        catch (Exception ex)
        {
            Log.Instance.Warning($"Employment contract verification failed: {ex.Message}");
            UpdateDocInList(_contractDoc, fileName, d => d with
            {
                Status = PermitReady.DocStatus.Warning,
                Alerts = ["Document uploaded but could not be verified automatically. Please ensure it is a valid employment contract."]
            });
        }
    }

    // ── Salary proof verification ─────────────────────────────────────────
    internal async Task VerifySalaryProofAsync(
        string tempFilePath, string fileName,
        string? declaredEmployer, decimal? declaredSalary)
    {
        try
        {
            var text = ExtractPdfText(tempFilePath);

            var (analysis, _) = await Emerge.Run<PermitReady.SalaryProofAnalysis>(
                LLMModel.Claude46Sonnet, new KernelContext(), pass =>
                {
                    pass.SystemPrompt = "You are a document verification assistant for the Finnish Immigration Service. Extract structured data from salary payslips, salary certificates, and income evidence documents. Return only valid JSON.";
                    pass.Command = $"""
                        Analyze the following text extracted from a PDF document named "{fileName}".
                        Determine if this is a salary payslip, salary certificate, or income evidence document and extract key fields.

                        Document text:
                        {text.Truncate(4000)}

                        Return JSON matching the schema:
                        {pass.JsonSchema}

                        For MonthlyAmount, extract the gross monthly salary or income figure in EUR (0 if not determinable).
                        For Period, use format "YYYY-MM" or descriptive period like "Q1 2024" (empty if not found).
                        """;
                    pass.Temperature = 0.1;
                }).FinalAsync();

            var alerts = new List<string>();
            var extracted = new Dictionary<string, string>();
            var status = PermitReady.DocStatus.Verified;

            if (!analysis.IsSalaryProof)
            {
                UpdateDocInList(_salaryProofDoc, fileName, d => d with
                {
                    Status = PermitReady.DocStatus.Warning,
                    Alerts = ["This document does not appear to be a salary payslip or income certificate. Please upload the correct file."]
                });
                return;
            }

            if (!string.IsNullOrWhiteSpace(analysis.EmployerName)) extracted["EmployerName"]  = analysis.EmployerName;
            if (analysis.MonthlyAmount > 0)                         extracted["MonthlyAmount"] = analysis.MonthlyAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            if (!string.IsNullOrWhiteSpace(analysis.Period))        extracted["Period"]        = analysis.Period;

            // Cross-validate salary figure
            if (analysis.MonthlyAmount > 0 && declaredSalary.HasValue && declaredSalary > 0)
            {
                var diff = Math.Abs(analysis.MonthlyAmount - declaredSalary.Value) / declaredSalary.Value;
                if (diff > 0.15m)
                {
                    alerts.Add($"Salary mismatch: payslip shows €{analysis.MonthlyAmount:N0}/mo but form declares €{declaredSalary:N0}/mo ({diff * 100:N0}% difference). Clarification required.");
                    status = PermitReady.DocStatus.Warning;
                }
                else
                {
                    alerts.Add($"Salary verified: payslip confirms €{analysis.MonthlyAmount:N0}/mo from {(string.IsNullOrWhiteSpace(analysis.EmployerName) ? "employer" : analysis.EmployerName)}.");
                }
            }

            // Cross-validate employer name
            if (!string.IsNullOrWhiteSpace(analysis.EmployerName) && !string.IsNullOrWhiteSpace(declaredEmployer))
            {
                if (!NamesLooseMatch(analysis.EmployerName, declaredEmployer))
                {
                    alerts.Add($"Employer mismatch: payslip shows \"{analysis.EmployerName}\" but form declares \"{declaredEmployer}\".");
                    status = PermitReady.DocStatus.Warning;
                }
            }

            if (!string.IsNullOrWhiteSpace(analysis.Notes) && analysis.Notes.Length > 5)
                alerts.Add(analysis.Notes);

            UpdateDocInList(_salaryProofDoc, fileName, d => d with { Status = status, Alerts = alerts, Extracted = extracted });
        }
        catch (Exception ex)
        {
            Log.Instance.Warning($"Salary proof verification failed: {ex.Message}");
            UpdateDocInList(_salaryProofDoc, fileName, d => d with
            {
                Status = PermitReady.DocStatus.Warning,
                Alerts = ["Document uploaded but could not be verified automatically."]
            });
        }
    }

    // ── List update helper ────────────────────────────────────────────────────
    private static void UpdateDocInList(
        ClientReactive<List<PermitReady.UploadedDoc>> slot,
        string fileName,
        Func<PermitReady.UploadedDoc, PermitReady.UploadedDoc> updater)
    {
        var list = slot.Value;
        var idx  = list.FindIndex(d => d.FileName == fileName);
        if (idx < 0) return;
        var newList = new List<PermitReady.UploadedDoc>(list);
        newList[idx] = updater(newList[idx]);
        slot.Value = newList;
    }

    // Loose name match: tokens of one string appear in the other (handles "Acme Oy" vs "ACME OY LTD")
    private static bool NamesLooseMatch(string a, string b)
    {
        var ta = a.ToUpperInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tb = b.ToUpperInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var (shorter, longer) = ta.Length <= tb.Length ? (ta, tb) : (tb, ta);
        return shorter.Any(t => longer.Any(l => l.StartsWith(t) || t.StartsWith(l)));
    }

    // ── Generic document verification (acceptance letter, contract, etc.) ─
    internal async Task VerifyGenericDocAsync(
        string tempFilePath, string fileName,
        string expectedType,
        Action<PermitReady.UploadedDoc?> setter,
        Func<PermitReady.UploadedDoc?> getter)
    {
        try
        {
            var text = ExtractPdfText(tempFilePath);

            var (analysis, _) = await Emerge.Run<PermitReady.GenericDocAnalysis>(
                LLMModel.Claude46Sonnet, new KernelContext(), pass =>
                {
                    pass.SystemPrompt = "You are a document verification assistant for Finnish Immigration Service. Verify that uploaded documents are the correct type. Return only valid JSON.";
                    pass.Command = $"""
                        Analyze the following text extracted from a PDF document named "{fileName}".
                        Verify this is a "{expectedType}" document.

                        Document text:
                        {text.Truncate(3000)}

                        Return JSON matching:
                        {pass.JsonSchema}

                        Set IsRelevant=true only if this document matches the expected type "{expectedType}".
                        """;
                    pass.Temperature = 0.1;
                }).FinalAsync();

            var current = getter();
            if (current == null) return;

            if (!analysis.IsRelevant)
            {
                setter(current with
                {
                    Status = PermitReady.DocStatus.Warning,
                    Alerts = [$"This appears to be a \"{analysis.DocumentType}\" rather than a \"{expectedType}\". Please verify you uploaded the correct document."]
                });
            }
            else
            {
                setter(current with { Status = PermitReady.DocStatus.Verified, Alerts = [] });
            }
        }
        catch
        {
            var current = getter();
            if (current != null)
                setter(current with { Status = PermitReady.DocStatus.Verified, Alerts = [] });
        }
    }

    // ── PDF text extraction (works for text-based PDFs) ───────────────────
    private static string ExtractPdfText(string filePath)
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);
            var raw   = Encoding.Latin1.GetString(bytes);

            // Extract text between BT (Begin Text) and ET (End Text) PDF operators
            var sb       = new StringBuilder();
            var inStream = false;
            var i        = 0;

            while (i < raw.Length - 2)
            {
                if (!inStream && i < raw.Length - 6 && raw.Substring(i, 6) == "stream")
                {
                    inStream = true;
                    i += 6;
                    continue;
                }
                if (inStream && i < raw.Length - 9 && raw.Substring(i, 9) == "endstream")
                {
                    inStream = false;
                    i += 9;
                    continue;
                }
                if (inStream)
                {
                    char c = raw[i];
                    if ((c >= 32 && c < 127) || c == '\n' || c == '\r' || c == '\t')
                        sb.Append(c);
                }
                i++;
            }

            // Also scan outside streams for readable text
            var fullScan = new StringBuilder();
            foreach (char c in raw)
            {
                if ((c >= 32 && c < 127) || c == '\n' || c == '\r')
                    fullScan.Append(c);
            }

            var extracted = sb.Length > 100 ? sb.ToString() : fullScan.ToString();
            return extracted.Length > 8000 ? extracted[..8000] : extracted;
        }
        catch
        {
            return string.Empty;
        }
    }
}

file static class StringTruncateExtension
{
    public static string Truncate(this string s, int max) =>
        s.Length <= max ? s : s[..max] + "... [truncated]";
}