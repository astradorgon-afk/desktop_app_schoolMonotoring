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

    /// <summary>True when this class has at least one schedule slot on today's weekday.</summary>
    public bool IsToday { get; init; }

    /// <summary>Earliest start time on today's schedule; null when it isn't scheduled today.</summary>
    public TimeOnly? StartsAt { get; init; }
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

/// <summary>One of the teacher's scheduled classes on a given day, and whether they held it.</summary>
public class TeacherAttendanceClass
{
    public long OfferingId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
    public string? Room { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }

    /// <summary>PRESENT (a session was held), ABSENT (time passed, no session), or UPCOMING.</summary>
    public string Status { get; init; } = "UPCOMING";

    /// <summary>When the session was actually started, if the teacher was present.</summary>
    public DateTime? HeldAt { get; init; }
}

/// <summary>A single day's worth of the teacher's own attendance across their scheduled classes.</summary>
public class TeacherAttendanceDay
{
    public DateOnly Date { get; init; }
    public List<TeacherAttendanceClass> Classes { get; init; } = [];
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

    /// <summary>
    /// Marks every session still flagged ACTIVE from a previous day as ENDED, and returns
    /// how many were closed. Sessions abandoned without pressing End would otherwise stay
    /// ACTIVE forever and keep showing as live in Attendance History.
    /// </summary>
    Task<int> CloseStaleSessionsAsync(CancellationToken cancellationToken = default);

    Task<ActiveSessionState> StartManualSessionAsync(long teacherId, long userId, long offeringId, CancellationToken cancellationToken = default);

    Task<ScanResult> ScanRoomQrAsync(long teacherId, long userId, string token, string? borrowPurpose = null, CancellationToken cancellationToken = default);

    Task<ActiveSessionState?> GetActiveSessionAsync(long sessionId, CancellationToken cancellationToken = default);

    Task<ScanResult> MarkAttendanceAsync(long sessionId, long userId, long studentId, CancellationToken cancellationToken = default);

    Task<ScanResult> ScanStudentQrAsync(long sessionId, long userId, string token, CancellationToken cancellationToken = default);

    Task<ScanResult> UndoAttendanceAsync(long sessionId, long userId, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Corrects a student's status in one of the teacher's sessions after the fact (mis-scan,
    /// forgotten card). A reason is required and the change is audited.
    /// </summary>
    Task<ScanResult> OverrideStatusAsync(long sessionId, long teacherId, long userId, long studentId, string status, string reason, CancellationToken cancellationToken = default);

    Task EndSessionAsync(long sessionId, long? userId = null, CancellationToken cancellationToken = default);

    Task<List<LiveScanEvent>> GetLiveScansAsync(long sessionId, CancellationToken cancellationToken = default);

    Task<List<RosterStudentItem>> GetRosterAsync(long sessionId, CancellationToken cancellationToken = default);

    Task<List<SessionHistoryItem>> GetAttendanceHistoryAsync(long teacherId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default);

    Task<List<SessionHistoryStudent>> GetSessionStudentsAsync(long sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The teacher's own attendance: for each scheduled class in the date range, whether they
    /// held a session (Present), missed it (Absent), or it is still upcoming. Grouped by day,
    /// newest first.
    /// </summary>
    Task<List<TeacherAttendanceDay>> GetTeacherAttendanceAsync(long teacherId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default);
}
