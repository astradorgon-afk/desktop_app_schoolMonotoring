namespace AttendanceUI.Data.Entities;

public class Section
{
    public long Id { get; set; }
    public long SchoolYearId { get; set; }
    public long GradeLevelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public long? AdviserTeacherId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsArchived { get; set; }
}
