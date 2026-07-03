namespace AttendanceUI.Services;

public class StudentWeeklyScheduleView
{
    public string Subject { get; init; } = string.Empty;
    public string Teacher { get; init; } = string.Empty;
    public string Day { get; init; } = string.Empty;
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
    public string? Room { get; init; }
}

public class StudentAttendanceHistoryView
{
    public DateTime Date { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Teacher { get; init; }
    public string? Reason { get; init; }
}

public class StudentDashboardData
{
    public long StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Lrn { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
    public byte[]? ProfilePicture { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string GradeLevel { get; init; } = string.Empty;
    public string CurrentSchoolYear { get; init; } = string.Empty;
    public int TotalSessions { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public double AttendanceRate { get; init; }
    public List<StudentWeeklyScheduleView> WeeklySchedule { get; init; } = [];
    public List<StudentWeeklyScheduleView> TodaySchedule { get; init; } = [];
    public List<StudentAttendanceHistoryView> RecentAttendance { get; init; } = [];
}

public interface IStudentPortalService
{
    Task<Data.Entities.Student?> GetStudentByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    Task<StudentDashboardData?> GetDashboardAsync(long studentId, CancellationToken cancellationToken = default);
}
