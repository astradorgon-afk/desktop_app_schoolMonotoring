using System.Globalization;
using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private readonly IAuditService _auditService;

    public LeaveRequestService(IDbContextFactory<SchoolDbContext> contextFactory, IAuditService auditService)
    {
        _contextFactory = contextFactory;
        _auditService = auditService;
    }

    public async Task<List<LeaveRequestView>> GetTeacherRequestsAsync(long teacherId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var items = await context.LeaveRequests
            .AsNoTracking()
            .Where(lr => lr.TeacherId == teacherId)
            .OrderByDescending(lr => lr.CreatedAt)
            .Select(lr => new
            {
                lr.Id,
                TeacherName = lr.Teacher!.LastName + ", " + lr.Teacher.FirstName,
                lr.Reason,
                lr.Category,
                lr.StartDate,
                lr.EndDate,
                lr.Status,
                lr.ReviewNotes,
                ReviewedByName = lr.ReviewedBy != null ? lr.ReviewedBy.Name : null,
                lr.CreatedAt
            })
            .ToListAsync(ct);

        return items.Select(lr => new LeaveRequestView
        {
            Id = lr.Id,
            TeacherName = lr.TeacherName,
            Reason = lr.Reason,
            Category = lr.Category,
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            DurationDays = (lr.EndDate.ToDateTime(TimeOnly.MinValue) - lr.StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1,
            Status = lr.Status,
            ReviewNotes = lr.ReviewNotes,
            ReviewedByName = lr.ReviewedByName,
            CreatedAt = lr.CreatedAt
        }).ToList();
    }

    public async Task<List<LeaveRequestView>> GetPendingRequestsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var items = await context.LeaveRequests
            .AsNoTracking()
            .Where(lr => lr.Status == "PENDING")
            .OrderByDescending(lr => lr.CreatedAt)
            .Select(lr => new
            {
                lr.Id,
                TeacherName = lr.Teacher!.LastName + ", " + lr.Teacher.FirstName,
                lr.Reason,
                lr.Category,
                lr.StartDate,
                lr.EndDate,
                lr.Status,
                lr.CreatedAt
            })
            .ToListAsync(ct);

        return items.Select(lr => new LeaveRequestView
        {
            Id = lr.Id,
            TeacherName = lr.TeacherName,
            Reason = lr.Reason,
            Category = lr.Category,
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            DurationDays = (lr.EndDate.ToDateTime(TimeOnly.MinValue) - lr.StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1,
            Status = lr.Status,
            CreatedAt = lr.CreatedAt
        }).ToList();
    }

    public async Task<List<LeaveRequestView>> GetAllRequestsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var items = await context.LeaveRequests
            .AsNoTracking()
            .OrderByDescending(lr => lr.CreatedAt)
            .Select(lr => new
            {
                lr.Id,
                TeacherName = lr.Teacher!.LastName + ", " + lr.Teacher.FirstName,
                lr.Reason, lr.Category, lr.StartDate, lr.EndDate, lr.Status,
                lr.ReviewNotes,
                ReviewedByName = lr.ReviewedBy != null ? lr.ReviewedBy.Name : null,
                lr.CreatedAt
            })
            .ToListAsync(ct);

        return items.Select(lr => new LeaveRequestView
        {
            Id = lr.Id, TeacherName = lr.TeacherName, Reason = lr.Reason,
            Category = lr.Category, StartDate = lr.StartDate, EndDate = lr.EndDate,
            DurationDays = (lr.EndDate.ToDateTime(TimeOnly.MinValue) - lr.StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1,
            Status = lr.Status, ReviewNotes = lr.ReviewNotes,
            ReviewedByName = lr.ReviewedByName, CreatedAt = lr.CreatedAt
        }).ToList();
    }

    public async Task<LeaveRequestDetail?> GetRequestDetailAsync(long requestId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var lr = await context.LeaveRequests
            .AsNoTracking()
            .Where(l => l.Id == requestId)
            .Select(l => new
            {
                l.Id,
                l.TeacherId,
                TeacherName = l.Teacher!.LastName + ", " + l.Teacher.FirstName,
                l.Reason,
                l.Category,
                l.StartDate,
                l.EndDate,
                l.Status,
                l.ReviewNotes,
                ReviewedByName = l.ReviewedBy != null ? l.ReviewedBy.Name : null,
                l.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (lr is null) return null;

        var schedules = await GetAffectedSchedulesAsync(lr.TeacherId, lr.StartDate, lr.EndDate, ct);

        return new LeaveRequestDetail
        {
            Id = lr.Id,
            TeacherId = lr.TeacherId,
            TeacherName = lr.TeacherName,
            Reason = lr.Reason,
            Category = lr.Category,
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            DurationDays = (lr.EndDate.ToDateTime(TimeOnly.MinValue) - lr.StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1,
            Status = lr.Status,
            ReviewNotes = lr.ReviewNotes,
            ReviewedByName = lr.ReviewedByName,
            CreatedAt = lr.CreatedAt,
            AffectedSchedules = schedules
        };
    }

    public async Task<LeaveRequest> SubmitAsync(SubmitLeaveRequest request, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var entity = new LeaveRequest
        {
            TeacherId = request.TeacherId,
            Reason = request.Reason,
            Category = request.Category,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.LeaveRequests.Add(entity);
        await context.SaveChangesAsync(ct);

        var teacher = await context.Teachers.FindAsync([request.TeacherId], ct);
        var teacherName = teacher is not null ? $"{teacher.LastName}, {teacher.FirstName}" : $"Teacher#{request.TeacherId}";
        await _auditService.LogAsync(request.TeacherId, teacherName, "TEACHER", "LeaveSubmit", "LeaveRequest", entity.Id,
            $"Leave request submitted: {request.Category} ({request.StartDate} - {request.EndDate})");

        return entity;
    }

    public async Task<bool> ApproveAsync(long requestId, long adminUserId, string? notes = null, string? adminName = null, string? adminRole = null, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var lr = await context.LeaveRequests.FindAsync([requestId], ct);
        if (lr is null || lr.Status != "PENDING") return false;

        lr.Status = "APPROVED";
        lr.ReviewedByUserId = adminUserId;
        lr.ReviewNotes = notes;
        lr.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        await _auditService.LogAsync(adminUserId, adminName ?? $"Admin#{adminUserId}", adminRole ?? "ADMIN", "LeaveApprove", "LeaveRequest", requestId,
            $"Leave request #{requestId} approved.");

        return true;
    }

    public async Task<bool> RejectAsync(long requestId, long adminUserId, string? notes = null, string? adminName = null, string? adminRole = null, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var lr = await context.LeaveRequests.FindAsync([requestId], ct);
        if (lr is null || lr.Status != "PENDING") return false;

        lr.Status = "REJECTED";
        lr.ReviewedByUserId = adminUserId;
        lr.ReviewNotes = notes;
        lr.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        await _auditService.LogAsync(adminUserId, adminName ?? $"Admin#{adminUserId}", adminRole ?? "ADMIN", "LeaveReject", "LeaveRequest", requestId,
            $"Leave request #{requestId} rejected.{(notes is not null ? $" Reason: {notes}" : "")}");

        return true;
    }

    public async Task<bool> CancelAsync(long requestId, long teacherId, string? teacherName = null, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var lr = await context.LeaveRequests.FindAsync([requestId], ct);
        if (lr is null || lr.TeacherId != teacherId || lr.Status != "PENDING") return false;

        context.LeaveRequests.Remove(lr);
        await context.SaveChangesAsync(ct);

        await _auditService.LogAsync(teacherId, teacherName ?? $"Teacher#{teacherId}", "TEACHER", "LeaveCancel", "LeaveRequest", requestId,
            $"Leave request #{requestId} cancelled.");

        return true;
    }

    public async Task<List<AffectedSchedule>> GetAffectedSchedulesAsync(
        long teacherId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var dayNumbers = new List<byte>();
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            var dow = (byte)(((int)d.DayOfWeek + 6) % 7 + 1);
            if (dow <= 5) dayNumbers.Add(dow);
        }

        dayNumbers = dayNumbers.Distinct().ToList();
        if (dayNumbers.Count == 0) return [];

        var offeringIds = await context.ClassOfferings
            .Where(co => co.TeacherId == teacherId)
            .Select(co => co.Id)
            .ToListAsync(ct);

        if (offeringIds.Count == 0) return [];

        var schedules = await context.ClassSchedules
            .AsNoTracking()
            .Where(cs => offeringIds.Contains(cs.ClassOfferingId))
            .Where(cs => dayNumbers.Contains(cs.DayOfWeek))
            .Select(cs => new
            {
                cs.ClassOffering!.Section!.Name,
                cs.ClassOffering.Subject!.Title,
                cs.DayOfWeek,
                cs.StartTime,
                cs.EndTime
            })
            .ToListAsync(ct);

        var dayNames = new[] { "", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        return schedules
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new AffectedSchedule
            {
                Section = s.Name,
                Subject = s.Title,
                Day = dayNames[s.DayOfWeek],
                StartTime = s.StartTime.ToString("h:mm tt", CultureInfo.InvariantCulture),
                EndTime = s.EndTime.ToString("h:mm tt", CultureInfo.InvariantCulture)
            })
            .ToList();
    }
}
