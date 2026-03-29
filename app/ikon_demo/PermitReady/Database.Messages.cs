using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PermitReady;

public static partial class PermitReadyDb
{
    private static readonly JsonSerializerOptions MsgJsonOpts = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    // ── Official Messages ─────────────────────────────────────────────────────

    public static async Task SaveOfficialMessageAsync(IAppBase host, OfficialMessage msg)
    {
        try
        {
            await using var conn = AppDatabaseConnection.Create(host, DbName);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO official_messages
                    (message_id, application_id, type, sender_role, content, sent_at,
                     read_by_officer, read_by_applicant, attachment_name)
                VALUES (@id, @appId, @type, @role, @content, @sent, @rofficer, @rapplicant, @attach)
                ON CONFLICT (message_id) DO UPDATE SET
                    read_by_officer   = EXCLUDED.read_by_officer,
                    read_by_applicant = EXCLUDED.read_by_applicant;
                """;
            P(cmd, "@id",         msg.MessageId);
            P(cmd, "@appId",      msg.ApplicationId);
            P(cmd, "@type",       msg.Type.ToString());
            P(cmd, "@role",       msg.SenderRole);
            P(cmd, "@content",    msg.Content);
            P(cmd, "@sent",       msg.SentAt);
            P(cmd, "@rofficer",   msg.ReadByOfficer);
            P(cmd, "@rapplicant", msg.ReadByApplicant);
            P(cmd, "@attach",     (object?)msg.AttachmentFileName ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }

    public static async Task<List<OfficialMessage>> LoadOfficialMessagesAsync(IAppBase host)
    {
        try
        {
            await using var conn = AppDatabaseConnection.Create(host, DbName);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT message_id, application_id, type, sender_role, content, sent_at,
                       read_by_officer, read_by_applicant, attachment_name
                FROM   official_messages
                ORDER  BY sent_at ASC
                """;
            var list = new List<OfficialMessage>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Enum.TryParse<OfficialMessageType>(reader.GetString(2), out var type);
                list.Add(new OfficialMessage(
                    MessageId:          reader.GetString(0),
                    ApplicationId:      reader.GetString(1),
                    Type:               type,
                    SenderRole:         reader.GetString(3),
                    Content:            reader.GetString(4),
                    SentAt:             reader.GetDateTime(5),
                    ReadByOfficer:      reader.GetBoolean(6),
                    ReadByApplicant:    reader.GetBoolean(7),
                    AttachmentFileName: reader.IsDBNull(8) ? null : reader.GetString(8)
                ));
            }
            return list;
        }
        catch { return []; }
    }

    // ── Profile Access Log ────────────────────────────────────────────────────

    public static async Task SaveProfileAccessAsync(IAppBase host, ProfileAccessEntry entry)
    {
        try
        {
            await using var conn = AppDatabaseConnection.Create(host, DbName);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO profile_access_log (application_id, accessed_at, reason)
                VALUES (@appId, @at, @reason);
                """;
            P(cmd, "@appId",  entry.ApplicationId);
            P(cmd, "@at",     entry.AccessedAt);
            P(cmd, "@reason", entry.Reason);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }
}
