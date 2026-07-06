namespace AttendanceUI.Services;

public class SectionSelectItem
{
    public long SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string GradeLevel { get; init; } = string.Empty;
    public int StudentCount { get; init; }
    public long? OfferingId { get; init; }
    public string? Subject { get; init; }
    public string? TimeDisplay { get; init; }
}

public class ActiveSessionState
{
    public long SessionId { get; init; }
    public long OfferingId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
    public string? Room { get; init; }
    public DateTime StartedAt { get; init; }
    public int TotalStudents { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public int UnmarkedCount { get; init; }
    public bool IsBorrowed { get; init; }
}

public class RosterStudentItem
{
    public long StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Lrn { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
    public string? Status { get; set; }
    public DateTime? ScannedAt { get; set; }
}

public class LiveScanEvent
{
    public long StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
}

public class SessionHistoryItem
{
    public long SessionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
    public string? Room { get; init; }
    public DateTime SessionDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public int Enrolled { get; init; }
    public int Present { get; init; }
    public int Late { get; init; }
    public int Absent { get; init; }
    public int Unmarked { get; init; }
    public double AttendanceRate => Enrolled > 0 ? Math.Round((double)(Present + Late) / Enrolled * 100, 1) : 0;
}

public class SessionHistoryStudent
{
    public long StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Lrn { get; init; } = string.Empty;
    public string? Status { get; set; }
    public DateTime? ScannedAt { get; set; }
}

public class ScanResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Status { get; init; }
    public string? StudentName { get; init; }
    public long? StudentId { get; init; }
}

public interface ITeacherSessionService
{
    Task<List<SectionSelectItem>> GetTeacherSectionsAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<ActiveSessionState?> GetActiveSessionForTeacherAsync(long teacherId, CancellationToken cancellationToken = default);

    Task<ActiveSessionState> StartManualSessionAsync(long teacherId, long userId, long offeringId, CancellationToken cancellationToken = default);

    Task<ScanResult> ScanRoomQrAsync(long teacherId, long userId, string token, string? borrowPurpose = null, CancellationToken cancellationToken = default);

    Task<ActiveSessionState?> GetActiveSessionAsync(long sessionId, CancellationToken cancellationToken = default);

    Task<ScanResult> MarkAttendanceAsync(long sessionId, long userId, long studentId, CancellationToken cancellationToken = default);

    Task<ScanResult> ScanStudentQrAsync(long sessionId, long userId, string token, CancellationToken cancellationToken = default);

    Task<ScanResult> UndoAttendanceAsync(long sessionId, long userId, long studentId, CancellationToken cancellationToken = default);

    Task EndSessionAsync(long sessionId, long? userId = null, CancellationToken cancellationToken = default);

    Task<List<LiveScanEvent>> GetLiveScansAsync(long sessionId, CancellationToken cancellationToken = default);

    Task<List<RosterStudentItem>> GetRosterAsync(long sessionId, CancellationToken cancellationToken = default);

    Task<List<SessionHistoryItem>> GetAttendanceHistoryAsync(long teacherId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default);

    Task<List<SessionHistoryStudent>> GetSessionStudentsAsync(long sessionId, CancellationToken cancellationToken = default);
}
