using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using AttendanceUI.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class StudentExcuseService : IStudentExcuseService
{
    private const string MigrationHint =
        "Excuse letters are not set up on this database yet. Ask the administrator to run Migrations/AddStudentExcuseRequestsTable.sql.";

    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private readonly IAuditService _auditService;

    public StudentExcuseService(IDbContextFactory<SchoolDbContext> contextFactory, IAuditService auditService)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<string?> EnsureAvailableAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await EnsureSchemaAsync(context, ct) ? null : MigrationHint;
    }

    public async Task<List<StudentExcuseView>> GetForStudentAsync(long studentId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await RequireSchemaAsync(context, ct);

        var query = context.StudentExcuseRequests.AsNoTracking().Where(e => e.StudentId == studentId);
        return await ProjectAsync(context, query, ct);
    }

    public async Task<StudentExcuseView> SubmitAsync(SubmitStudentExcuse request, UserSession actor, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Please describe the reason for the absence.");
        if (request.EndDate < request.StartDate)
            throw new InvalidOperationException("The end date cannot be before the start date.");
        if (request.EndDate > DateOnly.FromDateTime(DateTime.Today))
            throw new InvalidOperationException("Excuse letters can only cover days that have already started.");

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await RequireSchemaAsync(context, ct);

        var student = await context.Students.FindAsync([request.StudentId], ct)
            ?? throw new InvalidOperationException("Student not found.");

        var overlapping = await context.StudentExcuseRequests.AnyAsync(e =>
            e.StudentId == request.StudentId
            && e.Status != "REJECTED"
            && e.StartDate <= request.EndDate
            && e.EndDate >= request.StartDate, ct);
        if (overlapping)
            throw new InvalidOperationException("You already have a pending or approved excuse covering some of these dates.");

        var now = DateTime.UtcNow;
        var entity = new StudentExcuseRequest
        {
            StudentId = request.StudentId,
            Reason = request.Reason.Trim(),
            Category = request.Category,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "PENDING",
            Attachment = request.Attachment,
            AttachmentName = request.AttachmentName,
            AttachmentType = request.AttachmentType,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.StudentExcuseRequests.Add(entity);
        await context.SaveChangesAsync(ct);

        await _auditService.LogAsync(actor.UserId, actor.DisplayName, actor.Role.ToString(), "ExcuseSubmit", "StudentExcuse", entity.Id,
            $"{student.LastName}, {student.FirstName} filed an excuse: {request.Category} ({request.StartDate} - {request.EndDate})", ct: ct);

        return (await ProjectAsync(context, context.StudentExcuseRequests.AsNoTracking().Where(e => e.Id == entity.Id), ct)).Single();
    }

    public async Task<bool> CancelAsync(long requestId, long studentId, UserSession actor, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await RequireSchemaAsync(context, ct);

        var entity = await context.StudentExcuseRequests.FindAsync([requestId], ct);
        if (entity is null || entity.StudentId != studentId || entity.Status != "PENDING")
            return false;

        context.StudentExcuseRequests.Remove(entity);
        await context.SaveChangesAsync(ct);

        await _auditService.LogAsync(actor.UserId, actor.DisplayName, actor.Role.ToString(), "ExcuseCancel", "StudentExcuse", requestId,
            $"Excuse request #{requestId} cancelled by the student.", ct: ct);
        return true;
    }

    public async Task<List<StudentExcuseView>> GetForReviewAsync(UserSession reviewer, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await RequireSchemaAsync(context, ct);

        var query = context.StudentExcuseRequests.AsNoTracking();

        if (reviewer.Role == UserRole.TEACHER)
        {
            var studentIds = await TeacherScope.GetStudentIdsAsync(context, reviewer.UserId, ct);
            query = query.Where(e => studentIds.Contains(e.StudentId));
        }
        else if (reviewer.Role != UserRole.SUPERADMIN)
        {
            return [];
        }

        return await ProjectAsync(context, query, ct);
    }

    public async Task<ExcuseReviewResult> ApproveAsync(long requestId, UserSession reviewer, string? notes, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await RequireSchemaAsync(context, ct);

        var entity = await context.StudentExcuseRequests.FindAsync([requestId], ct);
        if (entity is null || entity.Status != "PENDING")
            return new ExcuseReviewResult { Message = "This request is no longer pending." };
        if (!await CanReviewAsync(context, reviewer, entity.StudentId, ct))
            return new ExcuseReviewResult { Message = "You can only review excuses for students in your sections." };

        entity.Status = "APPROVED";
        entity.ReviewedByUserId = reviewer.UserId;
        entity.ReviewNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        var fromUtc = entity.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();
        var toUtc = entity.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();
        var reason = ExcuseReason(entity.Category, entity.Reason);

        var absences = await context.AttendanceRecords
            .Where(r => r.StudentId == entity.StudentId
                && r.Status == "ABSENT"
                && r.Session!.SessionDate >= fromUtc
                && r.Session.SessionDate < toUtc)
            .ToListAsync(ct);

        foreach (var record in absences)
        {
            record.Status = "EXCUSED";
            record.Reason = reason;
            record.MarkedByUserId = reviewer.UserId;
            record.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(ct);

        await _auditService.LogAsync(reviewer.UserId, reviewer.DisplayName, reviewer.Role.ToString(), "ExcuseApprove", "StudentExcuse", requestId,
            $"Excuse request #{requestId} approved; {absences.Count} absence(s) marked EXCUSED.", ct: ct);

        return new ExcuseReviewResult
        {
            Success = true,
            RecordsExcused = absences.Count,
            Message = absences.Count == 0
                ? "Excuse approved. No recorded absences in that range yet — classes still in progress will record it as excused when they end."
                : $"Excuse approved. {absences.Count} absence(s) changed to Excused."
        };
    }

    public async Task<ExcuseReviewResult> RejectAsync(long requestId, UserSession reviewer, string? notes, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await RequireSchemaAsync(context, ct);

        var entity = await context.StudentExcuseRequests.FindAsync([requestId], ct);
        if (entity is null || entity.Status != "PENDING")
            return new ExcuseReviewResult { Message = "This request is no longer pending." };
        if (!await CanReviewAsync(context, reviewer, entity.StudentId, ct))
            return new ExcuseReviewResult { Message = "You can only review excuses for students in your sections." };

        entity.Status = "REJECTED";
        entity.ReviewedByUserId = reviewer.UserId;
        entity.ReviewNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        await _auditService.LogAsync(reviewer.UserId, reviewer.DisplayName, reviewer.Role.ToString(), "ExcuseReject", "StudentExcuse", requestId,
            $"Excuse request #{requestId} rejected.{(entity.ReviewNotes is not null ? $" Reason: {entity.ReviewNotes}" : "")}", ct: ct);

        return new ExcuseReviewResult { Success = true, Message = "Excuse rejected." };
    }

    public async Task<(byte[] Data, string Name, string Type)?> GetAttachmentAsync(long requestId, UserSession viewer, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await RequireSchemaAsync(context, ct);

        var entity = await context.StudentExcuseRequests.AsNoTracking()
            .Where(e => e.Id == requestId)
            .Select(e => new { e.StudentId, e.Attachment, e.AttachmentName, e.AttachmentType })
            .FirstOrDefaultAsync(ct);
        if (entity?.Attachment is null) return null;

        var allowed = viewer.Role == UserRole.STUDENT
            ? await context.Students.AnyAsync(s => s.Id == entity.StudentId && s.UserId == viewer.UserId, ct)
            : await CanReviewAsync(context, viewer, entity.StudentId, ct);
        if (!allowed) return null;

        return (entity.Attachment, entity.AttachmentName ?? "attachment", entity.AttachmentType ?? "application/octet-stream");
    }

    /// <summary>
    /// Used when a session ends: which of these students have an approved excuse covering
    /// <paramref name="day"/>, mapped to the reason to record. Returns an empty map if the
    /// excuse table is unavailable, so ending a session never fails because of this feature.
    /// </summary>
    internal static async Task<Dictionary<long, string>> GetExcusedStudentsAsync(
        SchoolDbContext context, IReadOnlyCollection<long> studentIds, DateOnly day, CancellationToken ct)
    {
        if (studentIds.Count == 0) return [];
        try
        {
            if (!await EnsureSchemaAsync(context, ct)) return [];

            var excuses = await context.StudentExcuseRequests.AsNoTracking()
                .Where(e => studentIds.Contains(e.StudentId)
                    && e.Status == "APPROVED"
                    && e.StartDate <= day
                    && e.EndDate >= day)
                .Select(e => new { e.StudentId, e.Category, e.Reason })
                .ToListAsync(ct);

            return excuses
                .GroupBy(e => e.StudentId)
                .ToDictionary(g => g.Key, g => ExcuseReason(g.First().Category, g.First().Reason));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return [];
        }
    }

    // ── Helpers ──

    private static string ExcuseReason(string? category, string reason)
    {
        var text = $"Excused{(string.IsNullOrWhiteSpace(category) ? "" : $" ({category})")}: {reason}";
        return text.Length > 250 ? text[..250] : text;
    }

    private static async Task RequireSchemaAsync(SchoolDbContext context, CancellationToken ct)
    {
        if (!await EnsureSchemaAsync(context, ct))
            throw new InvalidOperationException(MigrationHint);
    }

    private static Task<bool> EnsureSchemaAsync(SchoolDbContext context, CancellationToken ct) =>
        SchemaGuard.EnsureTableAsync(context, "student_excuse_requests", MySqlDdl, SqliteDdl,
            (c, t) => c.StudentExcuseRequests.AsNoTracking().AnyAsync(t), ct);

    private static async Task<bool> CanReviewAsync(SchoolDbContext context, UserSession reviewer, long studentId, CancellationToken ct)
    {
        if (reviewer.Role == UserRole.SUPERADMIN) return true;
        if (reviewer.Role != UserRole.TEACHER) return false;
        var studentIds = await TeacherScope.GetStudentIdsAsync(context, reviewer.UserId, ct);
        return studentIds.Contains(studentId);
    }

    private static async Task<List<StudentExcuseView>> ProjectAsync(SchoolDbContext context, IQueryable<StudentExcuseRequest> query, CancellationToken ct)
    {
        var rows = await query
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.Id,
                e.StudentId,
                e.Student!.FirstName,
                e.Student.LastName,
                e.Student.Lrn,
                e.Reason,
                e.Category,
                e.StartDate,
                e.EndDate,
                e.Status,
                e.ReviewNotes,
                ReviewedByName = e.ReviewedBy != null ? e.ReviewedBy.Name : null,
                HasAttachment = e.Attachment != null,
                e.AttachmentName,
                e.CreatedAt
            })
            .ToListAsync(ct);

        var studentIds = rows.Select(r => r.StudentId).Distinct().ToList();
        var activeYear = await context.SchoolYears
            .Where(y => y.Status == "ACTIVE")
            .Select(y => y.Id)
            .FirstOrDefaultAsync(ct);
        var sections = await context.Enrollments
            .Where(en => studentIds.Contains(en.StudentId) && en.SchoolYearId == activeYear && en.Status == "ACTIVE")
            .Join(context.Sections, en => en.SectionId, s => s.Id, (en, s) => new { en.StudentId, s.Name })
            .ToListAsync(ct);
        var sectionByStudent = sections
            .GroupBy(s => s.StudentId)
            .ToDictionary(g => g.Key, g => g.First().Name);

        return rows.Select(r => new StudentExcuseView
        {
            Id = r.Id,
            StudentId = r.StudentId,
            StudentName = $"{r.LastName}, {r.FirstName}",
            Lrn = r.Lrn,
            SectionName = sectionByStudent.GetValueOrDefault(r.StudentId),
            Reason = r.Reason,
            Category = r.Category,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            Status = r.Status,
            ReviewNotes = r.ReviewNotes,
            ReviewedByName = r.ReviewedByName,
            HasAttachment = r.HasAttachment,
            AttachmentName = r.AttachmentName,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    private const string MySqlDdl = """
        CREATE TABLE IF NOT EXISTS student_excuse_requests (
            Id BIGINT AUTO_INCREMENT PRIMARY KEY,
            student_id BIGINT NOT NULL,
            reason TEXT NOT NULL,
            category VARCHAR(50) NULL,
            start_date DATE NOT NULL,
            end_date DATE NOT NULL,
            status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
            reviewed_by_user_id BIGINT NULL,
            review_notes TEXT NULL,
            attachment LONGBLOB NULL,
            attachment_name VARCHAR(255) NULL,
            attachment_type VARCHAR(100) NULL,
            created_at DATETIME(6) NOT NULL,
            updated_at DATETIME(6) NOT NULL,
            INDEX idx_excuse_student (student_id),
            INDEX idx_excuse_status (status),
            CONSTRAINT fk_excuse_student FOREIGN KEY (student_id) REFERENCES students(Id) ON DELETE CASCADE,
            CONSTRAINT fk_excuse_reviewer FOREIGN KEY (reviewed_by_user_id) REFERENCES users(Id) ON DELETE SET NULL
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """;

    private const string SqliteDdl = """
        CREATE TABLE IF NOT EXISTS student_excuse_requests (
            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            student_id INTEGER NOT NULL REFERENCES students(Id) ON DELETE CASCADE,
            reason TEXT NOT NULL,
            category TEXT NULL,
            start_date TEXT NOT NULL,
            end_date TEXT NOT NULL,
            status TEXT NOT NULL DEFAULT 'PENDING',
            reviewed_by_user_id INTEGER NULL REFERENCES users(Id) ON DELETE SET NULL,
            review_notes TEXT NULL,
            attachment BLOB NULL,
            attachment_name TEXT NULL,
            attachment_type TEXT NULL,
            created_at TEXT NOT NULL,
            updated_at TEXT NOT NULL
        )
        """;
}
