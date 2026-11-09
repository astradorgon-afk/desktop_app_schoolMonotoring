namespace AttendanceUI.Services;

public class SectionAnalyticsStat
{
    public long SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string GradeLevel { get; init; } = string.Empty;
    public int TotalRecords { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public int ExcusedCount { get; init; }
    public double Percentage => TotalRecords > 0 ? Math.Round((double)(PresentCount + LateCount) / TotalRecords * 100, 1) : 0;
    public int EnrolledStudents { get; init; }
}

public class GradeLevelStat
{
    public long GradeLevelId { get; init; }
    public string GradeLevel { get; init; } = string.Empty;
    public int StudentCount { get; init; }
    public int TotalRecords { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public double Percentage => TotalRecords > 0 ? Math.Round((double)(PresentCount + LateCount) / TotalRecords * 100, 1) : 0;
}

/// <summary>Attendance for one local calendar day.</summary>
public class DailyTrendPoint
{
    public DateOnly Date { get; init; }
    public int Sessions { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public int ExcusedCount { get; init; }
    public int TotalRecords => PresentCount + LateCount + AbsentCount + ExcusedCount;
    public double Percentage => TotalRecords > 0 ? Math.Round((double)(PresentCount + LateCount) / TotalRecords * 100, 1) : 0;
}

/// <summary>
/// Narrows the analytics. Dates are local calendar days (inclusive); null ends are
/// unbounded. Section, grade level and subject narrow every attendance figure.
/// <paramref name="TeacherId"/> limits everything to that teacher's own classes
/// (the teacher Analytics page); it is not a user-chosen filter.
/// </summary>
public record AnalyticsFilter(
    DateOnly? From = null,
    DateOnly? To = null,
    long? SectionId = null,
    long? GradeLevelId = null,
    long? SubjectId = null,
    long? TeacherId = null)
{
    public static readonly AnalyticsFilter All = new();
}

public record AnalyticsOption(long Id, string Name, long? GradeLevelId = null);

public record AnalyticsFilterOptions(
    IReadOnlyList<AnalyticsOption> Sections,
    IReadOnlyList<AnalyticsOption> GradeLevels,
    IReadOnlyList<AnalyticsOption> Subjects,
    DateOnly? FirstSessionDate,
    DateOnly? LastSessionDate);

/// <summary>Attendance for one subject (a teacher's classes are told apart by subject).</summary>
public class SubjectAnalyticsStat
{
    public long SubjectId { get; init; }
    public string SubjectName { get; init; } = string.Empty;
    public int TotalRecords { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public int ExcusedCount { get; init; }
    public double Percentage => TotalRecords > 0 ? Math.Round((double)(PresentCount + LateCount) / TotalRecords * 100, 1) : 0;
}

public class AdminAnalyticsData
{
    public int TotalStudents { get; init; }
    public int TotalTeachers { get; init; }
    public int TotalSections { get; init; }
    public int TotalSubjects { get; init; }
    public int TotalSessions { get; init; }

    public int PresentToday { get; init; }
    public int LateToday { get; init; }
    public int AbsentToday { get; init; }
    public int TotalTodayRecords => PresentToday + LateToday + AbsentToday;

    public int TotalRecords { get; init; }
    public int TotalPresent { get; init; }
    public int TotalLate { get; init; }
    public int TotalAbsent { get; init; }
    public int TotalExcused { get; init; }

    public double OverallAttendanceRate => TotalRecords > 0 ? Math.Round((double)(TotalPresent + TotalLate) / TotalRecords * 100, 1) : 0;
    public double OnTimeRate => TotalRecords > 0 ? Math.Round((double)TotalPresent / TotalRecords * 100, 1) : 0;
    public double LateRate => TotalRecords > 0 ? Math.Round((double)TotalLate / TotalRecords * 100, 1) : 0;
    public double AbsentRate => TotalRecords > 0 ? Math.Round((double)TotalAbsent / TotalRecords * 100, 1) : 0;
    public double TodayRate => TotalTodayRecords > 0 ? Math.Round((double)PresentToday / TotalTodayRecords * 100, 1) : 0;

    public double ThisMonthRate { get; init; }
    public double LastMonthRate { get; init; }
    public double TrendChange => Math.Round(ThisMonthRate - LastMonthRate, 1);
    public int TotalStudentsWithRecords { get; init; }

    public List<SectionAnalyticsStat> SectionStats { get; init; } = [];
    public List<MonthlyTrendItem> MonthlyTrends { get; init; } = [];
    public List<DayOfWeekStat> DayOfWeekStats { get; init; } = [];
    public List<GradeLevelStat> GradeLevelStats { get; init; } = [];
    public List<DailyTrendPoint> DailyTrend { get; init; } = [];
    public List<SubjectAnalyticsStat> SubjectStats { get; init; } = [];

    /// <summary>The filter these figures were computed for.</summary>
    public AnalyticsFilter Filter { get; init; } = AnalyticsFilter.All;

    /// <summary>When the figures were read (local time).</summary>
    public DateTime GeneratedAt { get; init; } = DateTime.Now;
}

public interface IAnalyticsService
{
    /// <summary>All-time, school-wide figures (used by the dashboard).</summary>
    Task<AdminAnalyticsData> GetAdminAnalyticsAsync(CancellationToken ct = default);

    Task<AdminAnalyticsData> GetAdminAnalyticsAsync(AnalyticsFilter filter, CancellationToken ct = default);

    /// <summary>Filter choices; with <paramref name="teacherId"/>, only that teacher's sections and subjects.</summary>
    Task<AnalyticsFilterOptions> GetFilterOptionsAsync(long? teacherId = null, CancellationToken ct = default);
}
