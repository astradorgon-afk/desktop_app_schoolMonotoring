using AttendanceUI.Data.Entities;

namespace AttendanceUI.Models.Auth;

public class LoginResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public UserSession? Session { get; init; }

    public static LoginResponse Ok(UserSession session) =>
        new() { Success = true, Session = session };

    public static LoginResponse Fail(string message) =>
        new() { Success = false, Message = message };
}
