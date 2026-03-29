using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PermitReady;

/// <summary>
/// PostgreSQL persistence layer for PermitReady.
/// All methods are safe to call fire-and-forget — exceptions are swallowed.
/// Configure in ikon-config.toml: Databases = ["permitdb:postgres"]
/// </summary>
public static partial class PermitReadyDb
{
    private const string DbName = "permitdb";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        Converters         = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = null,
    };

    // ── Schema initialisation ─────────────────────────────────────────────────

    public static async Task InitializeAsync(IAppBase host)
    {
        await using var conn = AppDatabaseConnection.Create(host, DbName);
        await conn.OpenAsync();
        await using var cmd  = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS applications (
                application_id    TEXT PRIMARY KEY,
                input_json        TEXT NOT NULL,
                result_json       TEXT NOT NULL,
                submitted_at      TIMESTAMPTZ NOT NULL,
                status            TEXT NOT NULL DEFAULT 'Screened',
                status_changed_at TIMESTAMPTZ,
                officer_notes     TEXT
            );
            CREATE TABLE IF NOT EXISTS documents (
                application_id  TEXT NOT NULL,
                filename        TEXT NOT NULL,
                content_bytes   BYTEA NOT NULL,
                uploaded_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                PRIMARY KEY (application_id, filename)
            );
            CREATE TABLE IF NOT EXISTS chat_audit_log (
                id              BIGSERIAL PRIMARY KEY,
                application_id  TEXT NOT NULL,
                ts              TIMESTAMPTZ NOT NULL,
                user_query      TEXT NOT NULL,
                ai_response     TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS official_messages (
                message_id        TEXT PRIMARY KEY,
                application_id    TEXT NOT NULL,
                type              TEXT NOT NULL,
                sender_role       TEXT NOT NULL,
                content           TEXT NOT NULL,
                sent_at           TIMESTAMPTZ NOT NULL,
                read_by_officer   BOOLEAN NOT NULL DEFAULT FALSE,
                read_by_applicant BOOLEAN NOT NULL DEFAULT FALSE,
                attachment_name   TEXT
            );
            CREATE TABLE IF NOT EXISTS profile_access_log (
                id              BIGSERIAL PRIMARY KEY,
                application_id  TEXT NOT NULL,
                accessed_at     TIMESTAMPTZ NOT NULL,
                reason          TEXT NOT NULL
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    // ── Applications ──────────────────────────────────────────────────────────

    public static async Task SaveApplicationAsync(IAppBase host, StoredApplication app)
    {
        try
        {
            await using var conn = AppDatabaseConnection.Create(host, DbName);
            await conn.OpenAsync();

            var inputJson  = JsonSerializer.Serialize(app.Input,  JsonOpts);
            var resultJson = JsonSerializer.Serialize(app.Result, JsonOpts);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO applications
                    (application_id, input_json, result_json, submitted_at, status, status_changed_at, officer_notes)
                VALUES
                    (@id, @input, @result, @submitted, @status, @changed, @notes)
                ON CONFLICT (application_id) DO UPDATE SET
                    input_json        = EXCLUDED.input_json,
                    result_json       = EXCLUDED.result_json,
                    status            = EXCLUDED.status,
                    status_changed_at = EXCLUDED.status_changed_at,
                    officer_notes     = EXCLUDED.officer_notes;
                """;
            P(cmd, "@id",       app.ApplicationId);
            P(cmd, "@input",    inputJson);
            P(cmd, "@result",   resultJson);
            P(cmd, "@submitted",app.SubmittedAt);
            P(cmd, "@status",   app.Status.ToString());
            P(cmd, "@changed",  (object?)app.StatusChangedAt ?? DBNull.Value);
            P(cmd, "@notes",    (object?)app.OfficerNotes    ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { /* DB unavailable — in-memory state still works */ }
    }

    public static async Task<List<StoredApplication>> LoadApplicationsAsync(IAppBase host)
    {
        await using var conn = AppDatabaseConnection.Create(host, DbName);
        await conn.OpenAsync();
        await using var cmd  = conn.CreateCommand();
        cmd.CommandText = """
            SELECT application_id, input_json, result_json, submitted_at,
                   status, status_changed_at, officer_notes
            FROM   applications
            ORDER  BY submitted_at ASC
            """;

        var list = new List<StoredApplication>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var input  = JsonSerializer.Deserialize<ApplicationInput>(reader.GetString(1), JsonOpts)!;
            var result = JsonSerializer.Deserialize<ScreeningResult> (reader.GetString(2), JsonOpts)!;
            Enum.TryParse<ApplicationStatus>(reader.GetString(4), out var status);
            list.Add(new StoredApplication(
                ApplicationId:   reader.GetString(0),
                Input:           input,
                Result:          result,
                SubmittedAt:     reader.GetDateTime(3),
                Status:          status,
                StatusChangedAt: reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                OfficerNotes:    reader.IsDBNull(6) ? null : reader.GetString(6)
            ));
        }
        return list;
    }

    // ── Documents ─────────────────────────────────────────────────────────────

    public static async Task SaveDocumentAsync(IAppBase host, string appId, string filename, byte[] bytes)
    {
        try
        {
            await using var conn = AppDatabaseConnection.Create(host, DbName);
            await conn.OpenAsync();
            await using var cmd  = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO documents (application_id, filename, content_bytes)
                VALUES (@appId, @file, @bytes)
                ON CONFLICT (application_id, filename) DO UPDATE SET content_bytes = EXCLUDED.content_bytes;
                """;
            P(cmd, "@appId", appId);
            P(cmd, "@file",  filename);
            P(cmd, "@bytes", bytes);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }

    public static async Task<byte[]?> GetDocumentBytesAsync(IAppBase host, string appId, string filename)
    {
        try
        {
            await using var conn = AppDatabaseConnection.Create(host, DbName);
            await conn.OpenAsync();
            await using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT content_bytes FROM documents WHERE application_id = @appId AND filename = @file";
            P(cmd, "@appId", appId);
            P(cmd, "@file",  filename);
            var result = await cmd.ExecuteScalarAsync();
            return result is byte[] b ? b : null;
        }
        catch { return null; }
    }

    // ── Audit log ─────────────────────────────────────────────────────────────

    public static async Task SaveAuditEntryAsync(IAppBase host, ChatAuditEntry entry)
    {
        try
        {
            await using var conn = AppDatabaseConnection.Create(host, DbName);
            await conn.OpenAsync();
            await using var cmd  = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO chat_audit_log (application_id, ts, user_query, ai_response)
                VALUES (@appId, @ts, @query, @response);
                """;
            P(cmd, "@appId",    entry.ApplicationId);
            P(cmd, "@ts",       entry.Timestamp);
            P(cmd, "@query",    entry.UserQuery);
            P(cmd, "@response", entry.AiResponse);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }

    // ── Parameter helper ──────────────────────────────────────────────────────

    private static void P(DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }
}
