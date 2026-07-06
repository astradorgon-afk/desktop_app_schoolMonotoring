using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class AuditService : IAuditService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public AuditService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task LogAsync(long actorId, string actorName, string actorRole, string action,
        string entityType, long? entityId, string summary, string? metadata = null,
        CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        context.AuditLogs.Add(new AuditLogEntry
        {
            ActorId = actorId,
            ActorName = actorName,
            ActorRole = actorRole,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Summary = summary,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
    }

    public async Task<AuditLogResult> QueryAsync(AuditLogQuery query, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var q = context.AuditLogs.AsQueryable();

        if (query.DateFrom.HasValue)
        {
            var from = query.DateFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            q = q.Where(e => e.CreatedAt >= from);
        }
        if (query.DateTo.HasValue)
        {
            var to = query.DateTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            q = q.Where(e => e.CreatedAt <= to);
        }
        if (!string.IsNullOrWhiteSpace(query.ActorSearch))
        {
            var search = query.ActorSearch.Trim();
            q = q.Where(e => e.ActorName.Contains(search) || e.ActorId.ToString() == search);
        }
        if (!string.IsNullOrWhiteSpace(query.Action))
            q = q.Where(e => e.Action == query.Action);
        if (!string.IsNullOrWhiteSpace(query.EntityType))
            q = q.Where(e => e.EntityType == query.EntityType);

        var totalCount = await q.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var entries = await q
            .OrderByDescending(e => e.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new AuditLogResult
        {
            Entries = entries,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<int> GetTodayCountAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var todayStart = DateTime.Today.ToUniversalTime();
        return await context.AuditLogs.CountAsync(e => e.CreatedAt >= todayStart, ct);
    }

    public async Task<List<AuditActionStats>> GetActionBreakdownAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var todayStart = DateTime.Today.ToUniversalTime();
        return await context.AuditLogs
            .Where(e => e.CreatedAt >= todayStart)
            .GroupBy(e => e.Action)
            .Select(g => new AuditActionStats { Action = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .ToListAsync(ct);
    }
}
