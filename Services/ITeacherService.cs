using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public class TeacherScheduleView
{
    public string Section { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
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

public interface ITeacherService
{
    Task<IReadOnlyList<Teacher>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TeacherDetail?> GetDetailAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<Teacher?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    Task<TeacherDashboardData?> GetTeacherDashboardAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<List<TeacherSectionView>> GetHandledSectionsAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<List<SectionEnrolledStudent>> GetSectionStudentsAsync(long sectionId, CancellationToken cancellationToken = default);
}
