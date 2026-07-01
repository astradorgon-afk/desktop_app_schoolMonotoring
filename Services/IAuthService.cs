using AttendanceUI.Models.Auth;

namespace AttendanceUI.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync();

    Task<UserSession?> GetCurrentSessionAsync();

    bool IsAuthenticated { get; }
}
