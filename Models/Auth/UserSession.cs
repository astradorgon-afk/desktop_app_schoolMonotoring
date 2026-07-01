using AttendanceUI.Data.Entities;

namespace AttendanceUI.Models.Auth;

public class UserSession
{
    public long UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public DateTime LoggedInAt { get; init; } = DateTime.UtcNow;

    public bool IsInRole(UserRole role) => Role == role;

    public bool IsInAnyRole(params UserRole[] roles) =>
        roles.Contains(Role);
}
