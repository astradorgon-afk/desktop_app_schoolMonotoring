namespace AttendanceUI.Data.Entities;

public class Enrollment
{
    public long Id { get; set; }
    public long SchoolYearId { get; set; }
    public long StudentId { get; set; }
    public long GradeLevelId { get; set; }
    public long SectionId { get; set; }
    public long CurriculumId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
