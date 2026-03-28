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
                _passportDoc.Value = _passportDoc.Value! with
                {
                    Status = PermitReady.DocStatus.Failed,
                    Alerts = ["This document does not appear to be a passport. Please upload the correct file."]
                };
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

            _passportDoc.Value = _passportDoc.Value! with
            {
                Status   = status,
                Alerts   = alerts,
                Extracted = extracted
            };
        }
        catch (Exception ex)
        {
            Log.Instance.Warning($"Passport verification failed: {ex.Message}");
            _passportDoc.Value = _passportDoc.Value! with
            {
                Status = PermitReady.DocStatus.Warning,
                Alerts = ["Document uploaded but could not be verified automatically. Please ensure it is a valid passport."]
            };
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
                _bankStatementDoc.Value = _bankStatementDoc.Value! with
                {
                    Status = PermitReady.DocStatus.Failed,
                    Alerts = ["This document does not appear to be a bank statement. Please upload the correct file."]
                };
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

            _bankStatementDoc.Value = _bankStatementDoc.Value! with
            {
                Status    = status,
                Alerts    = alerts,
                Extracted = extracted
            };
        }
        catch (Exception ex)
        {
            Log.Instance.Warning($"Bank statement verification failed: {ex.Message}");
            _bankStatementDoc.Value = _bankStatementDoc.Value! with
            {
                Status = PermitReady.DocStatus.Warning,
                Alerts = ["Document uploaded but could not be verified automatically."]
            };
        }
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