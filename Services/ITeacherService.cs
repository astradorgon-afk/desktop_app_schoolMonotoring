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

public interface ITeacherService
{
    Task<IReadOnlyList<Teacher>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TeacherDetail?> GetDetailAsync(long teacherId, CancellationToken cancellationToken = default);
}
