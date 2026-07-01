namespace AttendanceUI.Data.Entities;

public class Teacher
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? EmployeeNo { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Email { get; set; }
    public string? ContactNo { get; set; }
    public DateTime? HireDate { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Specialization { get; set; }
    public string? AdvisoryAssignmentStatus { get; set; }
    public string? EmploymentStatus { get; set; }
    public byte[]? ProfilePicture { get; set; }

    public User? User { get; set; }
    public ICollection<ClassOffering> ClassOfferings { get; set; } = [];

    public string FullName =>
        $"{LastName}, {FirstName}{(MiddleName is not null ? $" {MiddleName}" : "")}";
}
