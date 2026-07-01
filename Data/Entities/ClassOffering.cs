namespace AttendanceUI.Data.Entities;

public class ClassOffering
{
    public long Id { get; set; }
    public long SchoolYearId { get; set; }
    public long SectionId { get; set; }
    public long SubjectId { get; set; }
    public long? TeacherId { get; set; }
    public long? CurriculumId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Room { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Teacher? Teacher { get; set; }
    public Section? Section { get; set; }
    public Subject? Subject { get; set; }
    public ICollection<ClassSchedule> Schedules { get; set; } = [];
}
