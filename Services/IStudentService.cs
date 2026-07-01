using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public interface IStudentService
{
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Student?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<StudentQr?> GetQrCodeAsync(long studentId, CancellationToken cancellationToken = default);

    Task<StudentQr> GenerateQrCodeAsync(long studentId, CancellationToken cancellationToken = default);
}
