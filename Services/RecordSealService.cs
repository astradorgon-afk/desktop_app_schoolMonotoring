using AttendanceUI.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

/// <summary>One attendance record and the result of checking its seal.</summary>
public record SealedRecordSample(long Id, string Student, DateOnly? SessionDate, string Status, string StoredSeal, SealState State);

/// <summary>Seal check over every attendance record, plus a few rows to show (altered ones first).</summary>
public record RecordSealOverview(int Intact, int Altered, int Unsealed, IReadOnlyList<SealedRecordSample> Samples);

public interface IRecordSealService
{
    /// <summary>Recomputes the HMAC seal of every attendance record and compares it to the stored one. Read-only.</summary>
    Task<RecordSealOverview> CheckAsync(int sampleLimit, CancellationToken ct = default);

    /// <summary>
    /// Seals records written before sealing existed. Records whose seal does not match are
    /// left alone, so this can never hide tampering. Returns the number of records sealed.
    /// </summary>
    Task<int> SealExistingAsync(long actorUserId, string actorName, string actorRole, CancellationToken ct = default);
}

public class RecordSealService(IDbContextFactory<SchoolDbContext> contextFactory, IAuditService auditService) : IRecordSealService
{
    private const int BatchSize = 500;

    public async Task<RecordSealOverview> CheckAsync(int sampleLimit, CancellationToken ct = default)
    {
        sampleLimit = Math.Clamp(sampleLimit, 1, 100);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var rows = await context.AttendanceRecords.AsNoTracking()
            .OrderByDescending(r => r.Id)
            .Select(r => new { r.Id, r.AttendanceSessionId, r.StudentId, r.Status, r.MarkedByUserId, r.Hash })
            .ToListAsync(ct);

        var states = rows
            .Select(r => (Row: r, State: RecordSeal.Check(r.AttendanceSessionId, r.StudentId, r.Status, r.MarkedByUserId, r.Hash)))
            .ToList();

        // Altered records first: they are the ones someone needs to look at.
        var shown = states
            .OrderBy(s => s.State == SealState.Altered ? 0 : 1)
            .Take(sampleLimit)
            .ToList();

        var ids = shown.Select(s => s.Row.Id).ToList();
        var details = await context.AttendanceRecords.AsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .Select(r => new
            {
                r.Id,
                Student = r.Student == null ? null : r.Student.LastName + ", " + r.Student.FirstName,
                SessionDate = r.Session == null ? (DateTime?)null : r.Session.SessionDate
            })
            .ToDictionaryAsync(r => r.Id, ct);

        var samples = shown.Select(s =>
        {
            details.TryGetValue(s.Row.Id, out var d);
            return new SealedRecordSample(
                s.Row.Id,
                d?.Student ?? $"Student #{s.Row.StudentId}",
                d?.SessionDate is { } date ? DateOnly.FromDateTime(date) : null,
                s.Row.Status,
                s.Row.Hash ?? "NULL",
                s.State);
        }).ToList();

        return new RecordSealOverview(
            states.Count(s => s.State == SealState.Intact),
            states.Count(s => s.State == SealState.Altered),
            states.Count(s => s.State == SealState.Unsealed),
            samples);
    }

    public async Task<int> SealExistingAsync(long actorUserId, string actorName, string actorRole, CancellationToken ct = default)
    {
        var updated = 0;
        long lastId = 0;
        while (true)
        {
            await using var context = await contextFactory.CreateDbContextAsync(ct);
            var batch = await context.AttendanceRecords
                .Where(r => r.Id > lastId && (r.Hash == null || !r.Hash.StartsWith(RecordSeal.Prefix)))
                .OrderBy(r => r.Id)
                .Take(BatchSize)
                .ToListAsync(ct);
            if (batch.Count == 0)
                break;

            foreach (var record in batch)
            {
                record.Hash = RecordSeal.Compute(record);
                // Written back unchanged so the database does not bump it on its own.
                context.Entry(record).Property(r => r.UpdatedAt).IsModified = true;
            }

            await context.SaveChangesAsync(ct);
            updated += batch.Count;
            lastId = batch[^1].Id;
        }

        await auditService.LogAsync(actorUserId, actorName, actorRole, "RecordSeal", "Database", null,
            $"Sealed {updated} existing attendance record{(updated == 1 ? "" : "s")} with HMAC-SHA256.", ct: ct);
        return updated;
    }
}
