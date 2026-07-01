using System.ComponentModel.DataAnnotations;

namespace AttendanceUI.Models.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required.")]
    [NotEmptyOrWhitespace(ErrorMessage = "Username cannot be empty.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
