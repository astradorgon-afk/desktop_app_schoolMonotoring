using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

/// <summary>School-wide attendance rules an administrator can tune.</summary>
public record AttendanceRules(int LateGraceMinutes)
{
    public static readonly AttendanceRules Default = new(15);
}

public interface IAttendanceSettingsService
{
    Task<AttendanceRules> GetRulesAsync(CancellationToken ct = default);

    /// <summary>Saves the rules; returns an error message, or null on success.</summary>
    Task<string?> SaveRulesAsync(AttendanceRules rules, long actorUserId, string actorName, string actorRole, CancellationToken ct = default);
}

public class AttendanceSettingsService : IAttendanceSettingsService
{
    internal const string LateGraceKey = "attendance.late_grace_minutes";

    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private readonly IAuditService _auditService;

    public AttendanceSettingsService(IDbContextFactory<SchoolDbContext> contextFactory, IAuditService auditService)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<AttendanceRules> GetRulesAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await ReadRulesAsync(context, ct);
    }

    public async Task<string?> SaveRulesAsync(AttendanceRules rules, long actorUserId, string actorName, string actorRole, CancellationToken ct = default)
    {
        if (rules.LateGraceMinutes is < 0 or > 120)
            return "Late grace period must be between 0 and 120 minutes.";

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        if (!await EnsureSchemaAsync(context, ct))
            return "Settings are not set up on this database yet. Ask the administrator to run Migrations/AddAppSettingsTable.sql.";

        var before = await ReadRulesAsync(context, ct);
        await UpsertAsync(context, LateGraceKey, rules.LateGraceMinutes, actorUserId, ct);
        await context.SaveChangesAsync(ct);

        await _auditService.LogAsync(actorUserId, actorName, actorRole, "SettingsUpdate", "AppSettings", null,
            $"Attendance rules changed: late grace period {before.LateGraceMinutes}→{rules.LateGraceMinutes} min.", ct: ct);
        return null;
    }

    /// <summary>
    /// Reads the rules with the given context. Falls back to the defaults when the settings
    /// table is missing or unreadable, so scanning never fails because of this feature.
    /// </summary>
    internal static async Task<AttendanceRules> ReadRulesAsync(SchoolDbContext context, CancellationToken ct)
    {
        try
        {
            if (!await EnsureSchemaAsync(context, ct)) return AttendanceRules.Default;

            var grace = await context.AppSettings.AsNoTracking()
                .Where(s => s.Key == LateGraceKey)
                .Select(s => s.Value)
                .FirstOrDefaultAsync(ct);

            return new AttendanceRules(ParseOr(grace, AttendanceRules.Default.LateGraceMinutes));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return AttendanceRules.Default;
        }
    }

    private static int ParseOr(string? value, int fallback) => int.TryParse(value, out var v) && v >= 0 ? v : fallback;

    private static async Task UpsertAsync(SchoolDbContext context, string key, int value, long actorUserId, CancellationToken ct)
    {
        var row = await context.AppSettings.FindAsync([key], ct);
        if (row is null)
        {
            row = new AppSetting { Key = key };
            context.AppSettings.Add(row);
        }
        row.Value = value.ToString();
        row.UpdatedByUserId = actorUserId;
        row.UpdatedAt = DateTime.UtcNow;
    }

    private static Task<bool> EnsureSchemaAsync(SchoolDbContext context, CancellationToken ct) =>
        SchemaGuard.EnsureTableAsync(context, "app_settings", MySqlDdl, SqliteDdl,
            (c, t) => c.AppSettings.AsNoTracking().AnyAsync(t), ct);

    private const string MySqlDdl = """
        CREATE TABLE IF NOT EXISTS app_settings (
            setting_key VARCHAR(100) NOT NULL PRIMARY KEY,
            setting_value TEXT NOT NULL,
            updated_by_user_id BIGINT NULL,
            updated_at DATETIME(6) NOT NULL
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """;

    private const string SqliteDdl = """
        CREATE TABLE IF NOT EXISTS app_settings (
            setting_key TEXT NOT NULL PRIMARY KEY,
            setting_value TEXT NOT NULL,
            updated_by_user_id INTEGER NULL,
            updated_at TEXT NOT NULL
        )
        """;
}
