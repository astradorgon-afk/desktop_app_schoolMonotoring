using AttendanceUI.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class ReportService : IReportService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public ReportService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Attendance between two local calendar days, inclusive. Session dates are stored as UTC,
    /// so the bounds are converted to UTC and every grouping uses the session's local date
    /// (a class at 7:30 AM local is still that day, not the previous UTC day).
    /// </summary>
    public async Task<ReportData> GetReportAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        // ── Available range: the earliest and latest local days that have sessions ──
        var firstStored = await context.AttendanceSessions.AsNoTracking().MinAsync(s => (DateTime?)s.SessionDate, ct);
        var lastStored = await context.AttendanceSessions.AsNoTracking().MaxAsync(s => (DateTime?)s.SessionDate, ct);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var defaultMin = firstStored is { } f ? SessionDates.LocalDate(f) : today;
        var defaultMax = lastStored is { } l ? SessionDates.LocalDate(l) : defaultMin;

        var fromDate = from ?? defaultMin;
        var toDate = to ?? defaultMax;
        if (fromDate > toDate)
            (fromDate, toDate) = (toDate, fromDate);

        var fromUtc = SessionDates.LocalDayStartUtc(fromDate);
        var untilUtc = SessionDates.LocalDayStartUtc(toDate.AddDays(1)); // exclusive

        // ── Sessions in range, with their section ──
        var sessions = await context.AttendanceSessions
            .AsNoTracking()
            .Where(s => s.SessionDate >= fromUtc && s.SessionDate < untilUtc)
            .Select(s => new { s.Id, s.SessionDate, SectionId = (long?)s.ClassOffering!.SectionId })
            .ToListAsync(ct);

        if (sessions.Count == 0)
        {
            return new ReportData
            {
                Summary = new ReportSummary(),
                SectionReports = [],
                DailyTrend = [],
                MinDate = defaultMin,
                MaxDate = defaultMax
            };
        }

        var sessionById = sessions.ToDictionary(s => s.Id);

        // ── Records in range (filtered by the same bounds in the database, no id list) ──
        var records = (await context.AttendanceRecords
                .AsNoTracking()
                .Where(r => r.Session!.SessionDate >= fromUtc && r.Session.SessionDate < untilUtc)
                .Select(r => new { r.AttendanceSessionId, r.Status })
                .ToListAsync(ct))
            .Where(r => sessionById.ContainsKey(r.AttendanceSessionId))
            .ToList();

        static int Count<T>(IEnumerable<T> items, Func<T, string> status, string value) => items.Count(i => status(i) == value);

        // ── Summary ──
        // Excused records stay out of the totals, so they never count against the rate.
        var presentCount = Count(records, r => r.Status, "PRESENT");
        var lateCount = Count(records, r => r.Status, "LATE");
        var absentCount = Count(records, r => r.Status, "ABSENT");

        var summary = new ReportSummary
        {
            TotalSessions = sessions.Count,
            TotalRecords = presentCount + lateCount + absentCount,
            PresentCount = presentCount,
            LateCount = lateCount,
            AbsentCount = absentCount
        };

        // ── Daily trend, by local calendar day ──
        var recordsBySession = records.ToLookup(r => r.AttendanceSessionId);
        var dailyTrend = sessions
            .GroupBy(s => SessionDates.LocalDate(s.SessionDate))
            .Select(g =>
            {
                var dayRecords = g.SelectMany(s => recordsBySession[s.Id]).ToList();
                return new DailyTrendItem
                {
                    Date = g.Key,
                    Sessions = g.Count(),
                    PresentCount = Count(dayRecords, r => r.Status, "PRESENT"),
                    LateCount = Count(dayRecords, r => r.Status, "LATE"),
                    AbsentCount = Count(dayRecords, r => r.Status, "ABSENT")
                };
            })
            .OrderByDescending(d => d.Date)
            .ToList();

        // ── Section breakdown (by section id: two sections may share a name across years) ──
        var sectionGroups = records
            .GroupBy(r => sessionById[r.AttendanceSessionId].SectionId)
            .Select(g => new
            {
                SectionId = g.Key,
                Total = g.Count(r => r.Status is "PRESENT" or "LATE" or "ABSENT"),
                Present = Count(g, r => r.Status, "PRESENT"),
                Late = Count(g, r => r.Status, "LATE"),
                Absent = Count(g, r => r.Status, "ABSENT")
            })
            .Where(g => g.Total > 0)
            .OrderByDescending(g => g.Total)
            .ToList();

        var sectionIds = sectionGroups.Where(g => g.SectionId is not null).Select(g => g.SectionId!.Value).ToList();
        var sectionNames = await context.Sections
            .AsNoTracking()
            .Where(s => sectionIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var enrollmentCounts = await context.Enrollments
            .AsNoTracking()
            .Where(e => sectionIds.Contains(e.SectionId) && (e.Status == "ACTIVE" || e.Status == "ENROLLED"))
            .GroupBy(e => e.SectionId)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SectionId, x => x.Count, ct);

        var sectionReports = sectionGroups.Select(g => new SectionReportItem
        {
            SectionName = g.SectionId is { } id && sectionNames.TryGetValue(id, out var name) ? name : "Unknown",
            EnrolledStudents = g.SectionId is { } sid ? enrollmentCounts.GetValueOrDefault(sid) : 0,
            TotalRecords = g.Total,
            PresentCount = g.Present,
            LateCount = g.Late,
            AbsentCount = g.Absent
        }).ToList();

        return new ReportData
        {
            Summary = summary,
            SectionReports = sectionReports,
            DailyTrend = dailyTrend,
            MinDate = defaultMin,
            MaxDate = defaultMax
        };
    }
}
