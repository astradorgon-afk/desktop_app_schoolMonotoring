using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public class StudentQrRow
{
    public long StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Lrn { get; init; } = string.Empty;
    public string StudentNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public long? SectionId { get; init; }
    public string? SectionName { get; init; }
    public long? GradeLevelId { get; init; }
    public string? GradeLevelName { get; init; }
    public string? QrContent { get; set; }
    public byte[]? QrImage { get; set; }
    public DateTime? QrUpdatedAt { get; set; }
}

public enum QrLookupKind
{
    Unknown,
    Student,
    Room,
    Token
}

public class QrLookupResult
{
    public QrLookupKind Kind { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public bool IsValid { get; init; }
}

public interface IQrCodeService
{
    /// <summary>Every student with their active-year section and current QR code (if any).</summary>
    Task<List<StudentQrRow>> GetStudentQrRowsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces a student's QR with a new one carrying a fresh suffix, so a lost or shared
    /// card stops scanning. Creates the QR if the student has none yet.
    /// </summary>
    Task<StudentQr> ReissueStudentQrAsync(long studentId, long actorUserId, string actorName, string actorRole,
        CancellationToken cancellationToken = default);

    /// <summary>Same as <see cref="ReissueStudentQrAsync"/> for a room QR.</summary>
    Task<RoomQr> ReissueRoomQrAsync(long roomId, long actorUserId, string actorName, string actorRole,
        CancellationToken cancellationToken = default);

    /// <summary>Identifies whose QR a scanned/pasted value belongs to, using the same matching as the scanner.</summary>
    Task<QrLookupResult> LookupAsync(string content, CancellationToken cancellationToken = default);
}
