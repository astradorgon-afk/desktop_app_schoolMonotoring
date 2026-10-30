using AttendanceUI.Models.Auth;

namespace AttendanceUI.Services;

public class StudentExcuseView
{
    public long Id { get; init; }
    public long StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string Lrn { get; init; } = string.Empty;
    public string? SectionName { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Category { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ReviewNotes { get; init; }
    public string? ReviewedByName { get; init; }
    public bool HasAttachment { get; init; }
    public string? AttachmentName { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsPending => Status == "PENDING";
    public int DurationDays => EndDate.DayNumber - StartDate.DayNumber + 1;
}

public class SubmitStudentExcuse
{
    public long StudentId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Category { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public byte[]? Attachment { get; init; }
    public string? AttachmentName { get; init; }
    public string? AttachmentType { get; init; }
}

public class ExcuseReviewResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int RecordsExcused { get; init; }
}

public interface IStudentExcuseService
{
    /// <summary>
    /// Makes sure the excuse table exists. Returns null when ready, or a message explaining
    /// what the admin must do (e.g. run the migration) when it cannot be created.
    /// </summary>
    Task<string?> EnsureAvailableAsync(CancellationToken ct = default);

    Task<List<StudentExcuseView>> GetForStudentAsync(long studentId, CancellationToken ct = default);

    Task<StudentExcuseView> SubmitAsync(SubmitStudentExcuse request, UserSession actor, CancellationToken ct = default);

    Task<bool> CancelAsync(long requestId, long studentId, UserSession actor, CancellationToken ct = default);

    /// <summary>Admins see every request; teachers see requests from students in sections they teach or advise.</summary>
    Task<List<StudentExcuseView>> GetForReviewAsync(UserSession reviewer, CancellationToken ct = default);

    /// <summary>Approves a pending request and turns the student's ABSENT records in that date range into EXCUSED.</summary>
    Task<ExcuseReviewResult> ApproveAsync(long requestId, UserSession reviewer, string? notes, CancellationToken ct = default);

    Task<ExcuseReviewResult> RejectAsync(long requestId, UserSession reviewer, string? notes, CancellationToken ct = default);

    Task<(byte[] Data, string Name, string Type)?> GetAttachmentAsync(long requestId, UserSession viewer, CancellationToken ct = default);
}
