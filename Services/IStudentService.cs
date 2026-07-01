using AttendanceUI.Models;

namespace AttendanceUI.Services;

public interface IStudentService
{
    Task<List<StudentListItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<StudentListItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<string> EnsureQrCodeAsync(long studentId, CancellationToken cancellationToken = default);
}
