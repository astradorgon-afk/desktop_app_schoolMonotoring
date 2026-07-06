using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public class LeaveRequestView
{
    public long Id { get; init; }
    public string TeacherName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string? Category { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public int DurationDays { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ReviewNotes { get; init; }
    public string? ReviewedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsPending => Status == "PENDING";
    public bool IsApproved => Status == "APPROVED";
    public bool IsRejected => Status == "REJECTED";
}

public class LeaveRequestDetail
{
    public long Id { get; init; }
    public long TeacherId { get; init; }
    public string TeacherName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string? Category { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public int DurationDays { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ReviewNotes { get; init; }
    public string? ReviewedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<AffectedSchedule> AffectedSchedules { get; init; } = [];
    public bool IsPending => Status == "PENDING";
    public bool IsApproved => Status == "APPROVED";
    public bool IsRejected => Status == "REJECTED";
}

public class AffectedSchedule
{
    public string Section { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Day { get; init; } = string.Empty;
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
}

public class SubmitLeaveRequest
{
    public long TeacherId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Category { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}

public static class LeaveCategories
{
    public static readonly string[] All = ["Sick Leave", "Business Leave", "Personal Leave", "Emergency Leave", "Other"];
}

public interface ILeaveRequestService
{
    Task<List<LeaveRequestView>> GetTeacherRequestsAsync(long teacherId, CancellationToken ct = default);

    Task<List<LeaveRequestView>> GetPendingRequestsAsync(CancellationToken ct = default);
    Task<List<LeaveRequestView>> GetAllRequestsAsync(CancellationToken ct = default);

    Task<LeaveRequestDetail?> GetRequestDetailAsync(long requestId, CancellationToken ct = default);

    Task<LeaveRequest> SubmitAsync(SubmitLeaveRequest request, CancellationToken ct = default);

    Task<bool> ApproveAsync(long requestId, long adminUserId, string? notes = null, string? adminName = null, string? adminRole = null, CancellationToken ct = default);

    Task<bool> RejectAsync(long requestId, long adminUserId, string? notes = null, string? adminName = null, string? adminRole = null, CancellationToken ct = default);

    Task<bool> CancelAsync(long requestId, long teacherId, string? teacherName = null, CancellationToken ct = default);

    Task<List<AffectedSchedule>> GetAffectedSchedulesAsync(long teacherId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default);
}
