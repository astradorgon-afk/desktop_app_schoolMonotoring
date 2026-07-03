namespace AttendanceUI.Services;

public class SessionStudentItem
{
    public long StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Lrn { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
    public string? Status { get; set; }
}

public class TeacherOfferingItem
{
    public long OfferingId { get; init; }
    public long SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string GradeLevel { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? Room { get; init; }
    public string TimeDisplay { get; init; } = string.Empty;
    public long? ExistingSessionId { get; init; }
}

public interface IAttendanceSessionService
{
    Task<List<TeacherOfferingItem>> GetTeacherOfferingsAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<long> GetOrCreateSessionAsync(long classOfferingId, long userId, CancellationToken cancellationToken = default);

    Task<List<SessionStudentItem>> GetSessionStudentsAsync(long sessionId, CancellationToken cancellationToken = default);

    Task MarkAttendanceAsync(long sessionId, long studentId, long userId, string status, CancellationToken cancellationToken = default);

    Task MarkAllPresentAsync(long sessionId, long userId, CancellationToken cancellationToken = default);

    Task<int> GetMarkedCountAsync(long sessionId, CancellationToken cancellationToken = default);
}
