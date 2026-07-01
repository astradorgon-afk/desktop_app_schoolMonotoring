using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public class RoomStatusItem
{
    public long RoomId { get; init; }
    public string RoomCode { get; init; } = string.Empty;
    public string RoomName { get; init; } = string.Empty;
    public int Capacity { get; init; }

    public long? OfferingId { get; init; }
    public string? SectionName { get; init; }
    public string? TeacherName { get; init; }
    public string? SubjectTitle { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }

    public int TotalStudents { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public bool HasSession { get; init; }
}

public interface ILiveMonitoringService
{
    Task<IReadOnlyList<RoomStatusItem>> GetTodayStatusAsync(CancellationToken ct = default);
}
