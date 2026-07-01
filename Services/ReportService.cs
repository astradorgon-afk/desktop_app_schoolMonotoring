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

    public async Task<ReportData> GetReportAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        // Determine available date range from data
        var dateBounds = await context.AttendanceSessions
            .AsNoTracking()
            .Select(s => (DateTime?)s.SessionDate)
            .FirstOrDefaultAsync(ct);

        var defaultMin = dateBounds.HasValue
            ? DateOnly.FromDateTime(dateBounds.Value)
            : DateOnly.FromDateTime(DateTime.Today);
        var defaultMax = defaultMin;

        // If we have sessions, get actual max date
        if (dateBounds.HasValue)
        {
            var maxDate = await context.AttendanceSessions
                .AsNoTracking()
                .MaxAsync(s => (DateTime?)s.SessionDate, ct);
            defaultMax = maxDate.HasValue ? DateOnly.FromDateTime(maxDate.Value) : defaultMin;
        }

        var fromDate = from ?? defaultMin;
        var toDate = to ?? defaultMax;
        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTime = toDate.ToDateTime(TimeOnly.MaxValue);

        // Get session IDs in range
        var sessionIds = await context.AttendanceSessions
            .AsNoTracking()
            .Where(s => s.SessionDate >= fromDateTime && s.SessionDate <= toDateTime)
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (sessionIds.Count == 0)
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

        // ── Summary ──
        var presentCount = await context.AttendanceRecords
            .AsNoTracking().CountAsync(r => sessionIds.Contains(r.AttendanceSessionId) && r.Status == "PRESENT", ct);
        var lateCount = await context.AttendanceRecords
            .AsNoTracking().CountAsync(r => sessionIds.Contains(r.AttendanceSessionId) && r.Status == "LATE", ct);
        var absentCount = await context.AttendanceRecords
            .AsNoTracking().CountAsync(r => sessionIds.Contains(r.AttendanceSessionId) && r.Status == "ABSENT", ct);
        var totalRecords = presentCount + lateCount + absentCount;

        var summary = new ReportSummary
        {
            TotalSessions = sessionIds.Distinct().Count(),
            TotalRecords = totalRecords,
            PresentCount = presentCount,
            LateCount = lateCount,
            AbsentCount = absentCount
        };

        // ── Daily Trend ──
        var trendRaw = await context.AttendanceSessions
            .AsNoTracking()
            .Where(s => s.SessionDate >= fromDateTime && s.SessionDate <= toDateTime)
            .Select(s => new
            {
                Date = s.SessionDate,
                Present = s.Records.Count(r => r.Status == "PRESENT"),
                Late = s.Records.Count(r => r.Status == "LATE"),
                Absent = s.Records.Count(r => r.Status == "ABSENT")
            })
            .ToListAsync(ct);

        var dailyTrend = trendRaw
            .GroupBy(x => DateOnly.FromDateTime(x.Date))
            .Select(g => new DailyTrendItem
            {
                Date = g.Key,
                Sessions = g.Count(),
                PresentCount = g.Sum(x => x.Present),
                LateCount = g.Sum(x => x.Late),
                AbsentCount = g.Sum(x => x.Absent)
            })
            .OrderByDescending(d => d.Date)
            .ToList();

        // ── Section Breakdown ──
        // Get offering → section mapping for sessions in range
        var sectionMap = await context.AttendanceSessions
            .AsNoTracking()
            .Where(s => sessionIds.Contains(s.Id))
            .Select(s => new { s.Id, SectionName = s.ClassOffering!.Section!.Name })
            .Distinct()
            .ToListAsync(ct);

        var sessionToSection = sectionMap.ToDictionary(s => s.Id, s => s.SectionName);

        // Get all attendance records for these sessions with their section mapping
        var sectionData = await context.AttendanceRecords
            .AsNoTracking()
            .Where(r => sessionIds.Contains(r.AttendanceSessionId))
            .Select(r => new { r.AttendanceSessionId, r.Status })
            .ToListAsync(ct);

        var sectionGroups = sectionData
            .GroupBy(r => sessionToSection.GetValueOrDefault(r.AttendanceSessionId, "Unknown"))
            .Select(g => new
            {
                SectionName = g.Key,
                Total = g.Count(),
                Present = g.Count(r => r.Status == "PRESENT"),
                Late = g.Count(r => r.Status == "LATE"),
                Absent = g.Count(r => r.Status == "ABSENT")
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        // Enrich with enrollment counts
        var secNames = sectionGroups.Select(s => s.SectionName).ToList();
        // Enrich with enrollment counts — simpler query
        var sectionLookup = await context.Sections
            .AsNoTracking()
            .Where(s => secNames.Contains(s.Name))
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);

        var sectionIds = sectionLookup.Select(s => s.Id).ToList();
        var enrollCounts = await context.Enrollments
            .AsNoTracking()
            .Where(e => sectionIds.Contains(e.SectionId) && e.Status == "ENROLLED")
            .GroupBy(e => e.SectionId)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var enrollmentCounts = sectionLookup.ToDictionary(
            s => s.Name,
            s => enrollCounts.FirstOrDefault(e => e.SectionId == s.Id)?.Count ?? 0);

        var sectionReports = sectionGroups.Select(g => new SectionReportItem
        {
            SectionName = g.SectionName,
            EnrolledStudents = enrollmentCounts.GetValueOrDefault(g.SectionName, 0),
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
