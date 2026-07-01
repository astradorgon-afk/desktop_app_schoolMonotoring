using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceUI.Data.Entities;

public enum UserRole
{
    SUPERADMIN,
    TEACHER,
    STUDENT
}

public enum UserStatus
{
    ACTIVE,
    INACTIVE,
    LOCKED
}

public class User
{
    public long Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }

    public bool CanLogin { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsActive => Status == UserStatus.ACTIVE && CanLogin;
}
