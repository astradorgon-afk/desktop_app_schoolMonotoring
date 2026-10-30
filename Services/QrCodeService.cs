using System.Security.Cryptography;
using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace AttendanceUI.Services;

public class QrCodeService : IQrCodeService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private readonly IAuditService _auditService;
    private readonly ILogger<QrCodeService> _logger;

    public QrCodeService(IDbContextFactory<SchoolDbContext> contextFactory, IAuditService auditService, ILogger<QrCodeService> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<StudentQrRow>> GetStudentQrRowsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var students = await context.Students
            .AsNoTracking()
            .Include(s => s.QrCode)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(cancellationToken);

        var activeYear = await context.SchoolYears
            .Where(y => y.Status == "ACTIVE")
            .Select(y => y.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var enrollments = activeYear == 0
            ? []
            : await context.Enrollments
                .AsNoTracking()
                .Where(e => e.SchoolYearId == activeYear && e.Status == "ACTIVE")
                .Select(e => new { e.StudentId, e.SectionId, e.GradeLevelId })
                .ToListAsync(cancellationToken);

        var enrollmentMap = enrollments
            .GroupBy(e => e.StudentId)
            .ToDictionary(g => g.Key, g => g.First());

        var sectionNames = await context.Sections
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        var gradeNames = await context.GradeLevels
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Id, g => g.Name, cancellationToken);

        return students.Select(s =>
        {
            enrollmentMap.TryGetValue(s.Id, out var enrollment);
            return new StudentQrRow
            {
                StudentId = s.Id,
                FullName = s.FullName,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Lrn = s.Lrn,
                StudentNumber = s.StudentNumber,
                Status = s.Status,
                SectionId = enrollment?.SectionId,
                SectionName = enrollment is not null ? sectionNames.GetValueOrDefault(enrollment.SectionId) : null,
                GradeLevelId = enrollment?.GradeLevelId,
                GradeLevelName = enrollment is not null ? gradeNames.GetValueOrDefault(enrollment.GradeLevelId) : null,
                QrContent = s.QrCode?.QrContent,
                QrImage = s.QrCode?.QrCode,
                QrUpdatedAt = s.QrCode?.UpdatedAt
            };
        }).ToList();
    }

    public async Task<StudentQr> ReissueStudentQrAsync(long studentId, long actorUserId, string actorName, string actorRole,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw new InvalidOperationException($"Student with ID {studentId} not found.");

        var qrContent = $"ATTENDANCE:{student.Lrn}:{student.StudentNumber}:{NewSuffix()}";
        var now = DateTime.UtcNow;

        var qr = await context.StudentQrCodes
            .FirstOrDefaultAsync(q => q.StudentId == studentId, cancellationToken);

        if (qr is null)
        {
            qr = new StudentQr { StudentId = studentId, CreatedAt = now };
            context.StudentQrCodes.Add(qr);
        }

        qr.QrContent = qrContent;
        qr.QrCode = RenderPng(qrContent);
        qr.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(actorUserId, actorName, actorRole, "QrReissue", "Student", studentId,
            $"Reissued QR code for {student.LastName}, {student.FirstName} ({student.Lrn}); previous card no longer scans.",
            ct: cancellationToken);

        _logger.LogInformation("QR code reissued for student ID {StudentId}.", studentId);
        return qr;
    }

    public async Task<RoomQr> ReissueRoomQrAsync(long roomId, long actorUserId, string actorName, string actorRole,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var room = await context.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken)
            ?? throw new InvalidOperationException($"Room with ID {roomId} not found.");

        var qrContent = $"ATTENDANCE:ROOM:{room.Code}:{room.Name}:{NewSuffix()}";
        var now = DateTime.UtcNow;

        var qr = await context.RoomQrCodes
            .FirstOrDefaultAsync(q => q.RoomId == roomId, cancellationToken);

        if (qr is null)
        {
            qr = new RoomQr { RoomId = roomId, CreatedAt = now };
            context.RoomQrCodes.Add(qr);
        }

        qr.QrContent = qrContent;
        qr.QrCode = RenderPng(qrContent);
        qr.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(actorUserId, actorName, actorRole, "QrReissue", "Room", roomId,
            $"Reissued QR code for room {room.Code} ({room.Name}); previous code no longer scans.",
            ct: cancellationToken);

        _logger.LogInformation("QR code reissued for room ID {RoomId}.", roomId);
        return qr;
    }

    public async Task<QrLookupResult> LookupAsync(string content, CancellationToken cancellationToken = default)
    {
        var value = content?.Trim() ?? string.Empty;
        if (value.Length == 0)
            return new QrLookupResult { Kind = QrLookupKind.Unknown, Title = "Nothing to check", Detail = "Scan or paste a QR value first." };

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var studentQr = await context.StudentQrCodes
            .AsNoTracking()
            .Include(q => q.Student)
            .FirstOrDefaultAsync(q => q.QrContent == value, cancellationToken);

        if (studentQr?.Student is { } student)
        {
            var active = student.Status == "ACTIVE";
            return new QrLookupResult
            {
                Kind = QrLookupKind.Student,
                IsValid = active,
                Title = student.FullName,
                Detail = $"Student · LRN {student.Lrn} · No. {student.StudentNumber}" +
                         (active ? " · Valid for scanning" : $" · Account is {student.Status}")
            };
        }

        var roomQr = await context.RoomQrCodes
            .AsNoTracking()
            .Include(q => q.Room)
            .FirstOrDefaultAsync(q => q.QrContent == value, cancellationToken);

        if (roomQr?.Room is { } room)
        {
            return new QrLookupResult
            {
                Kind = QrLookupKind.Room,
                IsValid = room.IsActive,
                Title = $"{room.Name} ({room.Code})",
                Detail = room.IsActive ? "Room · Valid for starting sessions" : "Room · Room is inactive"
            };
        }

        var token = await context.QrTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Token == value, cancellationToken);

        if (token is not null)
        {
            var valid = !token.IsRevoked && token.ExpiresAt > DateTime.UtcNow;
            return new QrLookupResult
            {
                Kind = QrLookupKind.Token,
                IsValid = valid,
                Title = $"{token.ModelType} token #{token.ModelId}",
                Detail = token.IsRevoked ? "Temporary token · Revoked"
                    : valid ? $"Temporary token · Expires {token.ExpiresAt.ToLocalTime():MMM dd, yyyy h:mm tt}"
                    : "Temporary token · Expired"
            };
        }

        return new QrLookupResult
        {
            Kind = QrLookupKind.Unknown,
            Title = "Not recognised",
            Detail = value.StartsWith("ATTENDANCE:", StringComparison.Ordinal)
                ? "This looks like an EduAttend code but it is no longer active — it was probably reissued."
                : "This QR code does not belong to any student or room."
        };
    }

    private static string NewSuffix() => Convert.ToHexString(RandomNumberGenerator.GetBytes(3));

    private static byte[] RenderPng(string content)
    {
        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var png = new PngByteQRCode(qrData);
        return png.GetGraphic(10);
    }
}
