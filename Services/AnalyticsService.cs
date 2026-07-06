using AttendanceUI.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public AnalyticsService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<AdminAnalyticsData> GetAdminAnalyticsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var totalStudents = await context.Students.CountAsync(s => s.Status == "ACTIVE", ct);
        var totalTeachers = await context.Teachers.CountAsync(t => t.Status == "ACTIVE", ct);
        var totalSections = await context.Sections.CountAsync(s => !s.IsArchived, ct);
        var totalSessions = await context.AttendanceSessions.CountAsync(ct);
        var totalRecords = await context.AttendanceRecords.CountAsync(ct);

        var today = DateTime.Today;
        var todayStart = today.ToUniversalTime();
        var todayEnd = todayStart.AddDays(1);

        var todaySessionIds = await context.AttendanceSessions
            .AsNoTracking()
            .Where(s => s.SessionDate >= todayStart && s.SessionDate < todayEnd)
            .Select(s => s.Id)
            .ToListAsync(ct);

        var presentToday = todaySessionIds.Count > 0
            ? await context.AttendanceRecords
                .AsNoTracking()
                .CountAsync(r => todaySessionIds.Contains(r.AttendanceSessionId) && r.Status == "PRESENT", ct)
            : 0;
        var lateToday = todaySessionIds.Count > 0
            ? await context.AttendanceRecords
                .AsNoTracking()
                .CountAsync(r => todaySessionIds.Contains(r.AttendanceSessionId) && r.Status == "LATE", ct)
            : 0;
        var absentToday = todaySessionIds.Count > 0
            ? await context.AttendanceRecords
                .AsNoTracking()
                .CountAsync(r => todaySessionIds.Contains(r.AttendanceSessionId) && r.Status == "ABSENT", ct)
            : 0;

        var totalPresent = await context.AttendanceRecords
            .AsNoTracking().CountAsync(r => r.Status == "PRESENT", ct);
        var totalLate = await context.AttendanceRecords
            .AsNoTracking().CountAsync(r => r.Status == "LATE", ct);
        var totalAbsent = await context.AttendanceRecords
            .AsNoTracking().CountAsync(r => r.Status == "ABSENT", ct);

        // Sessions with section mapping
        var allSessions = await context.AttendanceSessions
            .AsNoTracking()
            .Select(s => new { s.Id, s.SessionDate, SectionName = s.ClassOffering!.Section!.Name })
            .ToListAsync(ct);

        var sessionToSection = allSessions.ToDictionary(s => s.Id, s => s.SectionName);

        // Records
        var records = await context.AttendanceRecords
            .AsNoTracking()
            .Select(r => new { r.AttendanceSessionId, r.Status, r.StudentId })
            .ToListAsync(ct);

        // Section breakdown
        var sectionGroups = records
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

        var secNames = sectionGroups.Select(s => s.SectionName).ToList();
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

        var enrollmentLookup = sectionLookup.ToDictionary(
            s => s.Name,
            s => enrollCounts.FirstOrDefault(e => e.SectionId == s.Id)?.Count ?? 0);

        var sectionStats = sectionGroups.Select(g => new SectionAnalyticsStat
        {
            SectionName = g.SectionName,
            TotalRecords = g.Total,
            PresentCount = g.Present,
            LateCount = g.Late,
            AbsentCount = g.Absent,
            EnrolledStudents = enrollmentLookup.GetValueOrDefault(g.SectionName, 0)
        }).ToList();

        // Day-of-week stats
        var dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
        var dowRaw = records
            .GroupBy(r => allSessions.FirstOrDefault(s => s.Id == r.AttendanceSessionId)?.SessionDate.DayOfWeek)
            .Where(g => g.Key.HasValue && g.Key != DayOfWeek.Saturday && g.Key != DayOfWeek.Sunday)
            .Select(g =>
            {
                var total = g.Count();
                var present = g.Count(r => r.Status == "PRESENT");
                var late = g.Count(r => r.Status == "LATE");
                return new
                {
                    Day = g.Key!.Value.ToString(),
                    Percentage = total > 0 ? Math.Round((double)(present + late) / total * 100, 1) : 0
                };
            })
            .ToDictionary(d => d.Day, d => d.Percentage);

        var dowStats = dayNames
            .Select(d => new DayOfWeekStat { Day = d, Percentage = dowRaw.GetValueOrDefault(d, 0) })
            .ToList();

        // Monthly trends
        var monthlyData = records
            .GroupBy(r =>
            {
                var session = allSessions.FirstOrDefault(s => s.Id == r.AttendanceSessionId);
                return new { Year = session?.SessionDate.Year ?? 0, Month = session?.SessionDate.Month ?? 0 };
            })
            .Where(g => g.Key.Year > 0)
            .Select(g =>
            {
                var total = g.Count();
                var present = g.Count(r => r.Status == "PRESENT");
                var late = g.Count(r => r.Status == "LATE");
                return new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Percentage = total > 0 ? Math.Round((double)(present + late) / total * 100, 1) : 0
                };
            })
            .ToList();

        var monthLookup = monthlyData
            .Where(m => m.Year == today.Year)
            .ToDictionary(m => m.Month, m => m.Percentage);

        var monthlyTrends = Enumerable.Range(1, 12)
            .Select(m => new MonthlyTrendItem
            {
                Label = new DateTime(today.Year, m, 1).ToString("MMM"),
                Year = today.Year,
                Month = m,
                Percentage = monthLookup.GetValueOrDefault(m, 0)
            })
            .Where(m => m.Percentage > 0)
            .ToList();

        var monthsWithData = monthlyTrends.Where(m => m.Percentage > 0).ToList();
        var thisMonth = monthsWithData.LastOrDefault();
        var lastMonth = monthsWithData.Count >= 2 ? monthsWithData[^2] : null;

        // Students with records
        var studentsWithRecords = records.Select(r => r.StudentId).Distinct().Count();

        return new AdminAnalyticsData
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalSections = totalSections,
            TotalSessions = totalSessions,
            TotalRecords = totalRecords,
            PresentToday = presentToday,
            LateToday = lateToday,
            AbsentToday = absentToday,
            TotalPresent = totalPresent,
            TotalLate = totalLate,
            TotalAbsent = totalAbsent,
            SectionStats = sectionStats,
            MonthlyTrends = monthlyTrends,
            DayOfWeekStats = dowStats,
            GradeLevelStats = [],
            ThisMonthRate = thisMonth?.Percentage ?? 0,
            LastMonthRate = lastMonth?.Percentage ?? 0,
            TotalStudentsWithRecords = studentsWithRecords
        };
    }
}
