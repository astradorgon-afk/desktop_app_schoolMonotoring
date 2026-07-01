using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using AttendanceUI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace AttendanceUI.Services;

public class StudentService : IStudentService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IDbContextFactory<SchoolDbContext> contextFactory, ILogger<StudentService> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<StudentListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var students = await context.Students
            .Where(s => s.Status == "ACTIVE")
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(cancellationToken);

        var studentIds = students.Select(s => s.Id).ToList();
        var qrCodes = await context.StudentQrCodes
            .Where(q => studentIds.Contains(q.StudentId))
            .ToDictionaryAsync(q => q.StudentId, q => q.QrCode, cancellationToken);

        return students.Select(s => MapToListItem(s, qrCodes.GetValueOrDefault(s.Id))).ToList();
    }

    public async Task<StudentListItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var student = await context.Students.FindAsync([id], cancellationToken);
        if (student is null)
            return null;

        var qrCode = await context.StudentQrCodes
            .Where(q => q.StudentId == id)
            .Select(q => q.QrCode)
            .FirstOrDefaultAsync(cancellationToken);

        return MapToListItem(student, qrCode);
    }

    public async Task<string> EnsureQrCodeAsync(long studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await context.StudentQrCodes
            .Where(q => q.StudentId == studentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
            return Convert.ToBase64String(existing.QrCode);

        var student = await context.Students.FindAsync([studentId], cancellationToken);
        if (student is null)
            throw new InvalidOperationException($"Student with ID {studentId} not found.");

        var qrContent = $"ATTENDANCE:{student.Lrn}:{student.StudentNumber}";
        var qrBytes = GenerateQrCode(qrContent);

        var qrCode = new StudentQrCode
        {
            StudentId = studentId,
            QrContent = qrContent,
            QrCode = qrBytes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.StudentQrCodes.Add(qrCode);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex.InnerException is not null)
        {
            _logger.LogError(ex, "Failed to save QR code for student {StudentId}: {InnerMessage}",
                studentId, ex.InnerException.Message);
            throw;
        }

        _logger.LogInformation("Generated QR code for student {StudentId} ({Name})", studentId, student.DisplayName);

        return Convert.ToBase64String(qrBytes);
    }

    private static StudentListItem MapToListItem(Student s, byte[]? qrCodeBytes)
    {
        return new StudentListItem
        {
            Id = s.Id,
            StudentNumber = s.StudentNumber,
            Lrn = s.Lrn,
            FirstName = s.FirstName,
            LastName = s.LastName,
            MiddleName = s.MiddleName,
            Sex = s.Sex,
            Age = s.Age,
            ContactNo = s.ContactNo,
            Address = s.Address,
            GuardianName = s.GuardianName,
            GuardianContact = s.GuardianContact,
            Status = s.Status,
            HasQrCode = qrCodeBytes is not null && qrCodeBytes.Length > 0,
            QrCodeBase64 = qrCodeBytes is not null ? Convert.ToBase64String(qrCodeBytes) : null
        };
    }

    private static byte[] GenerateQrCode(string content)
    {
        using var generator = new QRCodeGenerator();
        var qrData = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var png = new PngByteQRCode(qrData);
        return png.GetGraphic(10);
    }
}
