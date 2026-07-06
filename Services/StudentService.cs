using System.Resources;
using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using AttendanceUI.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace AttendanceUI.Services;

public class StudentService : IStudentService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private readonly ILogger<StudentService> _logger;
    private readonly ResourceManager _logMessages = new(typeof(LogMessages));

    public StudentService(IDbContextFactory<SchoolDbContext> contextFactory, ILogger<StudentService> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Students
            .Include(s => s.QrCode)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Student?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Students
            .Include(s => s.QrCode)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<StudentQr?> GetQrCodeAsync(long studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.StudentQrCodes
            .FirstOrDefaultAsync(q => q.StudentId == studentId, cancellationToken);
    }

    public async Task<StudentQr> GenerateQrCodeAsync(long studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw new InvalidOperationException($"Student with ID {studentId} not found.");

        var existing = await context.StudentQrCodes
            .FirstOrDefaultAsync(q => q.StudentId == studentId, cancellationToken);

        if (existing is not null)
            return existing;

        var qrContent = $"ATTENDANCE:{student.Lrn}:{student.StudentNumber}";

        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(10);

        var qr = new StudentQr
        {
            StudentId = studentId,
            QrContent = qrContent,
            QrCode = qrBytes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.StudentQrCodes.Add(qr);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            _logMessages.GetString("QR_Generated") ?? "QR code generated for student ID {StudentId}.",
            studentId);

        return qr;
    }

    public async Task<IReadOnlyList<Section>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Sections
            .Where(s => !s.IsArchived)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GradeLevel>> GetGradeLevelsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.GradeLevels
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<long, (long? SectionId, long? GradeLevelId)>> GetStudentEnrollmentMapAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var activeYear = await context.SchoolYears
            .Where(y => y.Status == "ACTIVE")
            .Select(y => y.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeYear == 0)
            return [];

        var enrollments = await context.Enrollments
            .Where(e => e.SchoolYearId == activeYear && e.Status == "ACTIVE")
            .Select(e => new { e.StudentId, e.SectionId, e.GradeLevelId })
            .ToListAsync(cancellationToken);

        return enrollments.ToDictionary(e => e.StudentId, e => ((long?)e.SectionId, (long?)e.GradeLevelId));
    }
}
