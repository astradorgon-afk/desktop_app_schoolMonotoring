using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> CreateAsync(User user, string plainPassword, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    Task<bool> ValidateCredentialsAsync(string username, string plainPassword, CancellationToken cancellationToken = default);

    Task<bool> ResetPasswordAsync(long userId, string newPassword, CancellationToken cancellationToken = default);

    Task<bool> ToggleLockAsync(long userId, CancellationToken cancellationToken = default);
}
