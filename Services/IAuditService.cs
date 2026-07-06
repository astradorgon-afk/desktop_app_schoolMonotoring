using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public class AuditLogQuery
{
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }
    public string? ActorSearch { get; init; }
    public string? Action { get; init; }
    public string? EntityType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public class AuditLogResult
{
    public List<AuditLogEntry> Entries { get; init; } = [];
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public class AuditActionStats
{
    public string Action { get; init; } = string.Empty;
    public int Count { get; init; }
}

public interface IAuditService
{
    Task LogAsync(long actorId, string actorName, string actorRole, string action,
        string entityType, long? entityId, string summary, string? metadata = null,
        CancellationToken ct = default);

    Task<AuditLogResult> QueryAsync(AuditLogQuery query, CancellationToken ct = default);

    Task<int> GetTodayCountAsync(CancellationToken ct = default);

    Task<List<AuditActionStats>> GetActionBreakdownAsync(CancellationToken ct = default);
}
