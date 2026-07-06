namespace AttendanceUI.Services;

public class SectionAnalyticsStat
{
    public string SectionName { get; init; } = string.Empty;
    public int TotalRecords { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public double Percentage => TotalRecords > 0 ? Math.Round((double)(PresentCount + LateCount) / TotalRecords * 100, 1) : 0;
    public int EnrolledStudents { get; init; }
}

public class GradeLevelStat
{
    public string GradeLevel { get; init; } = string.Empty;
    public int StudentCount { get; init; }
    public int TotalRecords { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public double Percentage => TotalRecords > 0 ? Math.Round((double)(PresentCount + LateCount) / TotalRecords * 100, 1) : 0;
}

public class AdminAnalyticsData
{
    public int TotalStudents { get; init; }
    public int TotalTeachers { get; init; }
    public int TotalSections { get; init; }
    public int TotalSessions { get; init; }

    public int PresentToday { get; init; }
    public int LateToday { get; init; }
    public int AbsentToday { get; init; }
    public int TotalTodayRecords => PresentToday + LateToday + AbsentToday;

    public int TotalRecords { get; init; }
    public int TotalPresent { get; init; }
    public int TotalLate { get; init; }
    public int TotalAbsent { get; init; }

    public double OverallAttendanceRate => TotalRecords > 0 ? Math.Round((double)(TotalPresent + TotalLate) / TotalRecords * 100, 1) : 0;
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
}

public interface IAnalyticsService
{
    Task<AdminAnalyticsData> GetAdminAnalyticsAsync(CancellationToken ct = default);
}
