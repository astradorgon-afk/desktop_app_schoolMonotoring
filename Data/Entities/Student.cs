using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceUI.Data.Entities;

public class Student
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? ProfileImageUrl { get; set; }

    public string Lrn { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public DateTime? Birthdate { get; set; }

    public string? Sex { get; set; }

    public string? Address { get; set; }

    public string? GuardianName { get; set; }

    public string? GuardianContact { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string StudentNumber { get; set; } = string.Empty;

    public int? Age { get; set; }

    public string? ContactNo { get; set; }

    public string? PreviousSchool { get; set; }

    public long? PreferredGradeLevelId { get; set; }

    public long? PreferredCurriculumId { get; set; }

    public byte[]? ProfilePicture { get; set; }

    public User? User { get; set; }

    public StudentQr? QrCode { get; set; }

    public string FullName =>
        $"{LastName}, {FirstName}{(MiddleName is not null ? $" {MiddleName}" : "")}";
}
