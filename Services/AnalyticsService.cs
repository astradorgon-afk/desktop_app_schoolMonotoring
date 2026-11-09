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

    public Task<AdminAnalyticsData> GetAdminAnalyticsAsync(CancellationToken ct = default) =>
        GetAdminAnalyticsAsync(AnalyticsFilter.All, ct);

    // Session dates are stored as UTC (DateTime.UtcNow), so local calendar-day bounds are
    // converted to UTC before comparing, and grouping uses the session's local date.
    private static DateTime LocalDayStartUtc(DateOnly day) => SessionDates.LocalDayStartUtc(day);

    private static DateOnly LocalDate(DateTime storedUtc) => SessionDates.LocalDate(storedUtc);

    // Some databases mark live enrollments ACTIVE, the SQLite seed marks them ENROLLED.
    private static bool IsLiveEnrollment(string status) => status == "ACTIVE" || status == "ENROLLED";

    public async Task<AdminAnalyticsData> GetAdminAnalyticsAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        filter ??= AnalyticsFilter.All;
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        // ── Sections (with their grade level) and class offerings ──
        // Offerings tie each session to its section, subject and teacher; the table is small,
        // so it is read once and every scope check below happens in memory.
        var sectionInfo = await (
                from s in context.Sections.AsNoTracking()
                join g in context.GradeLevels.AsNoTracking() on s.GradeLevelId equals g.Id into gj
                from g in gj.DefaultIfEmpty()
                select new { s.Id, s.Name, s.GradeLevelId, GradeName = g != null ? g.Name : "", s.IsArchived })
            .ToListAsync(ct);
        var sectionById = sectionInfo.ToDictionary(s => s.Id);

        var offerings = await context.ClassOfferings.AsNoTracking()
            .Select(o => new { o.Id, o.SectionId, o.SubjectId, o.TeacherId })
            .ToListAsync(ct);
        var offeringById = offerings.ToDictionary(o => o.Id);

        var teacherSectionIds = filter.TeacherId is { } tid
            ? offerings.Where(o => o.TeacherId == tid).Select(o => o.SectionId).ToHashSet()
            : null;

        var scopedSectionIds = sectionInfo
            .Where(s => teacherSectionIds is null || teacherSectionIds.Contains(s.Id))
            .Where(s => filter.SectionId is null || s.Id == filter.SectionId)
            .Where(s => filter.GradeLevelId is null || s.GradeLevelId == filter.GradeLevelId)
            .Select(s => s.Id)
            .ToHashSet();
        var sectionScoped = filter.SectionId is not null || filter.GradeLevelId is not null || filter.TeacherId is not null;
        var anyScope = sectionScoped || filter.SubjectId is not null;

        var scopedOfferingIds = offerings
            .Where(o => filter.TeacherId is null || o.TeacherId == filter.TeacherId)
            .Where(o => scopedSectionIds.Contains(o.SectionId))
            .Where(o => filter.SubjectId is null || o.SubjectId == filter.SubjectId)
            .Select(o => o.Id)
            .ToHashSet();

        bool InScope(long offeringId) => !anyScope || scopedOfferingIds.Contains(offeringId);

        // With a subject chosen, the headline counts cover only the sections taking that subject.
        var countedSectionIds = filter.SubjectId is null
            ? scopedSectionIds
            : offerings.Where(o => scopedOfferingIds.Contains(o.Id)).Select(o => o.SectionId).ToHashSet();

        // ── Headline counts ──
        var enrollments = await context.Enrollments.AsNoTracking()
            .Select(e => new { e.SectionId, e.StudentId, e.Status })
            .ToListAsync(ct);
        var liveEnrollments = enrollments.Where(e => IsLiveEnrollment(e.Status)).ToList();

        var totalStudents = anyScope
            ? liveEnrollments.Where(e => countedSectionIds.Contains(e.SectionId)).Select(e => e.StudentId).Distinct().Count()
            : await context.Students.CountAsync(s => s.Status == "ACTIVE", ct);
        var totalTeachers = await context.Teachers.CountAsync(t => t.Status == "ACTIVE", ct);
        var totalSections = sectionInfo.Count(s => !s.IsArchived && countedSectionIds.Contains(s.Id));
        var totalSubjects = offerings
            .Where(o => (filter.TeacherId is null || o.TeacherId == filter.TeacherId) && countedSectionIds.Contains(o.SectionId))
            .Where(o => filter.SubjectId is null || o.SubjectId == filter.SubjectId)
            .Select(o => o.SubjectId)
            .Distinct()
            .Count();

        // ── Sessions in scope ──
        var sessionQuery = context.AttendanceSessions.AsNoTracking();
        if (filter.From is { } from)
        {
            var fromUtc = LocalDayStartUtc(from);
            sessionQuery = sessionQuery.Where(s => s.SessionDate >= fromUtc);
        }
        if (filter.To is { } to)
        {
            var toUtc = LocalDayStartUtc(to.AddDays(1));
            sessionQuery = sessionQuery.Where(s => s.SessionDate < toUtc);
        }

        var sessions = (await sessionQuery
                .Select(s => new { s.Id, s.SessionDate, s.ClassOfferingId })
                .ToListAsync(ct))
            .Where(s => InScope(s.ClassOfferingId))
            .Select(s =>
            {
                var o = offeringById.GetValueOrDefault(s.ClassOfferingId);
                return new { s.Id, s.SessionDate, SectionId = o?.SectionId ?? 0, SubjectId = o?.SubjectId ?? 0 };
            })
            .ToList();

        var sessionById = sessions.ToDictionary(s => s.Id);

        // ── Records in scope ──
        // Records are few compared with sessions, so they are read once and matched in memory
        // (avoids sending thousands of session ids back to the database).
        var allRecords = await context.AttendanceRecords.AsNoTracking()
            .Select(r => new { r.AttendanceSessionId, r.Status, r.StudentId })
            .ToListAsync(ct);

        var records = allRecords
            .Where(r => sessionById.ContainsKey(r.AttendanceSessionId))
            .Select(r =>
            {
                var s = sessionById[r.AttendanceSessionId];
                return new { r.Status, r.StudentId, s.SectionId, s.SubjectId, Date = LocalDate(s.SessionDate), SessionId = s.Id };
            })
            .ToList();

        int Count(IEnumerable<string> statuses, string status) => statuses.Count(x => x == status);

        // ── Today (local), within the section/subject/teacher scope but ignoring the date range ──
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayStartUtc = LocalDayStartUtc(today);
        var todayEndUtc = LocalDayStartUtc(today.AddDays(1));
        var todaySessionIds = (await context.AttendanceSessions.AsNoTracking()
                .Where(s => s.SessionDate >= todayStartUtc && s.SessionDate < todayEndUtc)
                .Select(s => new { s.Id, s.ClassOfferingId })
                .ToListAsync(ct))
            .Where(s => InScope(s.ClassOfferingId))
            .Select(s => s.Id)
            .ToHashSet();
        var todayStatuses = allRecords.Where(r => todaySessionIds.Contains(r.AttendanceSessionId)).Select(r => r.Status).ToList();

        // ── Subject breakdown ──
        var subjectIds = records.Select(r => r.SubjectId).Distinct().ToList();
        var subjectNames = await context.Subjects.AsNoTracking()
            .Where(su => subjectIds.Contains(su.Id))
            .Select(su => new { su.Id, su.Title })
            .ToDictionaryAsync(su => su.Id, su => su.Title, ct);

        var subjectStats = records
            .GroupBy(r => r.SubjectId)
            .Select(g =>
            {
                var statuses = g.Select(r => r.Status).ToList();
                return new SubjectAnalyticsStat
                {
                    SubjectId = g.Key,
                    SubjectName = subjectNames.GetValueOrDefault(g.Key) is { Length: > 0 } n ? n : "Unknown subject",
                    TotalRecords = statuses.Count,
                    PresentCount = Count(statuses, "PRESENT"),
                    LateCount = Count(statuses, "LATE"),
                    AbsentCount = Count(statuses, "ABSENT"),
                    ExcusedCount = Count(statuses, "EXCUSED")
                };
            })
            .OrderBy(s => s.SubjectName, NaturalComparer.Instance)
            .ToList();

        // ── Section breakdown ──
        var enrolledBySection = liveEnrollments
            .GroupBy(e => e.SectionId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.StudentId).Distinct().Count());

        var sectionStats = records
            .GroupBy(r => r.SectionId)
            .Select(g =>
            {
                var info = sectionById.GetValueOrDefault(g.Key);
                var statuses = g.Select(r => r.Status).ToList();
                return new SectionAnalyticsStat
                {
                    SectionId = g.Key,
                    SectionName = info?.Name ?? "Unknown",
                    GradeLevel = info?.GradeName ?? "",
                    TotalRecords = statuses.Count,
                    PresentCount = Count(statuses, "PRESENT"),
                    LateCount = Count(statuses, "LATE"),
                    AbsentCount = Count(statuses, "ABSENT"),
                    ExcusedCount = Count(statuses, "EXCUSED"),
                    EnrolledStudents = enrolledBySection.GetValueOrDefault(g.Key)
                };
            })
            .OrderByDescending(s => s.TotalRecords)
            .ThenBy(s => s.SectionName)
            .ToList();

        // ── Grade level breakdown ──
        var gradeStats = records
            .Where(r => sectionById.ContainsKey(r.SectionId))
            .GroupBy(r => sectionById[r.SectionId].GradeLevelId)
            .Select(g =>
            {
                var statuses = g.Select(r => r.Status).ToList();
                var gradeSections = sectionInfo.Where(s => s.GradeLevelId == g.Key).Select(s => s.Id).ToHashSet();
                return new GradeLevelStat
                {
                    GradeLevelId = g.Key,
                    GradeLevel = sectionInfo.FirstOrDefault(s => s.GradeLevelId == g.Key)?.GradeName is { Length: > 0 } name ? name : $"Grade #{g.Key}",
                    StudentCount = liveEnrollments.Where(e => gradeSections.Contains(e.SectionId)).Select(e => e.StudentId).Distinct().Count(),
                    TotalRecords = statuses.Count,
                    PresentCount = Count(statuses, "PRESENT"),
                    LateCount = Count(statuses, "LATE"),
                    AbsentCount = Count(statuses, "ABSENT")
                };
            })
            .OrderBy(g => g.GradeLevel, NaturalComparer.Instance)
            .ToList();

        // ── Daily trend (local days with sessions or records) ──
        var sessionsPerDay = sessions.GroupBy(s => LocalDate(s.SessionDate)).ToDictionary(g => g.Key, g => g.Count());
        var dailyTrend = records
            .GroupBy(r => r.Date)
            .Select(g =>
            {
                var statuses = g.Select(r => r.Status).ToList();
                return new DailyTrendPoint
                {
                    Date = g.Key,
                    Sessions = sessionsPerDay.GetValueOrDefault(g.Key),
                    PresentCount = Count(statuses, "PRESENT"),
                    LateCount = Count(statuses, "LATE"),
                    AbsentCount = Count(statuses, "ABSENT"),
                    ExcusedCount = Count(statuses, "EXCUSED")
                };
            })
            .OrderBy(d => d.Date)
            .ToList();

        // ── Day of week (school days) ──
        var dayNames = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        var dowStats = dayNames
            .Select(day =>
            {
                var statuses = records.Where(r => r.Date.DayOfWeek == day).Select(r => r.Status).ToList();
                var attended = Count(statuses, "PRESENT") + Count(statuses, "LATE");
                return new DayOfWeekStat
                {
                    Day = day.ToString(),
                    Percentage = statuses.Count > 0 ? Math.Round((double)attended / statuses.Count * 100, 1) : 0
                };
            })
            .ToList();

        // ── Monthly trend (every month with data, across years) ──
        var monthlyTrends = records
            .GroupBy(r => new { r.Date.Year, r.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g =>
            {
                var statuses = g.Select(r => r.Status).ToList();
                var attended = Count(statuses, "PRESENT") + Count(statuses, "LATE");
                return new MonthlyTrendItem
                {
                    Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Percentage = Math.Round((double)attended / statuses.Count * 100, 1)
                };
            })
            .ToList();

        var thisMonth = monthlyTrends.LastOrDefault();
        var lastMonth = monthlyTrends.Count >= 2 ? monthlyTrends[^2] : null;

        var allStatuses = records.Select(r => r.Status).ToList();

        return new AdminAnalyticsData
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalSections = totalSections,
            TotalSubjects = totalSubjects,
            TotalSessions = sessions.Count,
            TotalRecords = allStatuses.Count,
            PresentToday = Count(todayStatuses, "PRESENT"),
            LateToday = Count(todayStatuses, "LATE"),
            AbsentToday = Count(todayStatuses, "ABSENT"),
            TotalPresent = Count(allStatuses, "PRESENT"),
            TotalLate = Count(allStatuses, "LATE"),
            TotalAbsent = Count(allStatuses, "ABSENT"),
            TotalExcused = Count(allStatuses, "EXCUSED"),
            SectionStats = sectionStats,
            GradeLevelStats = gradeStats,
            DailyTrend = dailyTrend,
            SubjectStats = subjectStats,
            MonthlyTrends = monthlyTrends,
            DayOfWeekStats = dowStats,
            ThisMonthRate = thisMonth?.Percentage ?? 0,
            LastMonthRate = lastMonth?.Percentage ?? 0,
            TotalStudentsWithRecords = records.Select(r => r.StudentId).Distinct().Count(),
            Filter = filter,
            GeneratedAt = DateTime.Now
        };
    }

    public async Task<AnalyticsFilterOptions> GetFilterOptionsAsync(long? teacherId = null, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var grades = await context.GradeLevels.AsNoTracking()
            .Select(g => new { g.Id, g.Name })
            .ToListAsync(ct);

        var offerings = await context.ClassOfferings.AsNoTracking()
            .Where(o => teacherId == null || o.TeacherId == teacherId)
            .Select(o => new { o.Id, o.SectionId, o.SubjectId })
            .ToListAsync(ct);
        var offeringSectionIds = offerings.Select(o => o.SectionId).ToHashSet();
        var offeringSubjectIds = offerings.Select(o => o.SubjectId).ToHashSet();

        var sections = (await context.Sections.AsNoTracking()
                .Where(s => !s.IsArchived)
                .Select(s => new { s.Id, s.Name, s.GradeLevelId })
                .ToListAsync(ct))
            .Where(s => teacherId is null || offeringSectionIds.Contains(s.Id))
            .ToList();

        var subjects = (await context.Subjects.AsNoTracking()
                .Select(su => new { su.Id, su.Title })
                .ToListAsync(ct))
            .Where(su => offeringSubjectIds.Contains(su.Id))
            .ToList();

        var gradeName = grades.ToDictionary(g => g.Id, g => g.Name);
        var usedGrades = sections.Select(s => s.GradeLevelId).ToHashSet();

        // The data range of this scope, so the custom period can start from real data.
        DateTime? first, last;
        if (teacherId is null)
        {
            first = await context.AttendanceSessions.AsNoTracking().MinAsync(s => (DateTime?)s.SessionDate, ct);
            last = await context.AttendanceSessions.AsNoTracking().MaxAsync(s => (DateTime?)s.SessionDate, ct);
        }
        else
        {
            var offeringIds = offerings.Select(o => o.Id).ToList();
            var mine = context.AttendanceSessions.AsNoTracking().Where(s => offeringIds.Contains(s.ClassOfferingId));
            first = await mine.MinAsync(s => (DateTime?)s.SessionDate, ct);
            last = await mine.MaxAsync(s => (DateTime?)s.SessionDate, ct);
        }

        return new AnalyticsFilterOptions(
            sections
                .Select(s => new AnalyticsOption(s.Id,
                    gradeName.TryGetValue(s.GradeLevelId, out var g) && g.Length > 0 ? $"{g} — {s.Name}" : s.Name,
                    s.GradeLevelId))
                .OrderBy(s => s.Name, NaturalComparer.Instance)
                .ToList(),
            grades
                .Where(g => usedGrades.Contains(g.Id))
                .Select(g => new AnalyticsOption(g.Id, g.Name))
                .OrderBy(g => g.Name, NaturalComparer.Instance)
                .ToList(),
            subjects
                .Select(su => new AnalyticsOption(su.Id, su.Title))
                .OrderBy(su => su.Name, NaturalComparer.Instance)
                .ToList(),
            first is { } f ? LocalDate(f) : null,
            last is { } l ? LocalDate(l) : null);
    }

    /// <summary>Orders "Grade 7" before "Grade 10" (numbers compared by value).</summary>
    private sealed class NaturalComparer : IComparer<string>
    {
        public static readonly NaturalComparer Instance = new();

        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            int i = 0, j = 0;
            while (i < x.Length && j < y.Length)
            {
                if (char.IsDigit(x[i]) && char.IsDigit(y[j]))
                {
                    var si = i; while (i < x.Length && char.IsDigit(x[i])) i++;
                    var sj = j; while (j < y.Length && char.IsDigit(y[j])) j++;
                    var a = x[si..i].TrimStart('0'); var b = y[sj..j].TrimStart('0');
                    var c = a.Length != b.Length ? a.Length.CompareTo(b.Length) : string.CompareOrdinal(a, b);
                    if (c != 0) return c;
                }
                else
                {
                    var c = char.ToUpperInvariant(x[i]).CompareTo(char.ToUpperInvariant(y[j]));
                    if (c != 0) return c;
                    i++; j++;
                }
            }
            return (x.Length - i).CompareTo(y.Length - j);
        }
    }
}
