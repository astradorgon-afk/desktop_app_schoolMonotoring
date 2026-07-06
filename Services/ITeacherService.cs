using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public class TeacherScheduleView
{
    public string Section { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public long SubjectId { get; init; }
    public string Day { get; init; } = string.Empty;
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
    public string? Room { get; init; }
}

public class TeacherDetail
{
    public Teacher Teacher { get; init; } = null!;
    public List<TeacherScheduleView> Schedules { get; init; } = [];
}

public class TeacherDashboardData
{
    public Teacher Teacher { get; init; } = null!;
    public int TotalClassesToday { get; init; }
    public int TotalStudents { get; init; }
    public int PresentToday { get; init; }
    public int LateToday { get; init; }
    public int AbsentToday { get; init; }
    public List<TeacherScheduleView> TodaySchedule { get; init; } = [];
    public List<TeacherScheduleView> WeeklySchedule { get; init; } = [];
}

public class TeacherSectionView
{
    public long SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string GradeLevel { get; init; } = string.Empty;
    public int StudentCount { get; init; }
    public List<string> Subjects { get; init; } = [];
    public List<TeacherScheduleView> Schedules { get; init; } = [];
}

public class SectionEnrolledStudent
{
    public long StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Lrn { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
}

public class DayOfWeekStat
{
    public string Day { get; init; } = string.Empty;
    public double Percentage { get; init; }
}

public class SectionAttendanceStat
{
    public string SectionName { get; init; } = string.Empty;
    public double Percentage { get; init; }
    public int Present { get; init; }
    public int Late { get; init; }
    public int Absent { get; init; }
    public int Total => Present + Late + Absent;
}

public class MonthlyTrendItem
{
    public string Label { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Month { get; init; }
    public double Percentage { get; init; }
}

public class TeacherAnalyticsData
{
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
    public int TotalStudents { get; init; }
    public double ThisMonthRate { get; init; }
    public double LastMonthRate { get; init; }
    public double TrendChange => Math.Round(ThisMonthRate - LastMonthRate, 1);
    public List<DayOfWeekStat> DayOfWeekStats { get; init; } = [];
    public List<SectionAttendanceStat> SectionStats { get; init; } = [];
    public List<MonthlyTrendItem> MonthlyTrends { get; init; } = [];
}

public interface ITeacherService
{
    Task<IReadOnlyList<Teacher>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TeacherDetail?> GetDetailAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<Teacher?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    Task<TeacherDashboardData?> GetTeacherDashboardAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<List<TeacherScheduleView>> GetWeeklyScheduleAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<List<TeacherSectionView>> GetHandledSectionsAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<List<SectionEnrolledStudent>> GetSectionStudentsAsync(long sectionId, CancellationToken cancellationToken = default);

    Task<TeacherAnalyticsData> GetTeacherAnalyticsAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Teacher teacher, CancellationToken cancellationToken = default);

    Task<bool> UpdateProfilePictureAsync(long teacherId, byte[] picture, CancellationToken cancellationToken = default);
}
