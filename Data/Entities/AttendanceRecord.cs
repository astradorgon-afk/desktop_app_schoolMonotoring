namespace AttendanceUI.Data.Entities;

public class AttendanceRecord
{
    public long Id { get; set; }
    public long AttendanceSessionId { get; set; }
    public long StudentId { get; set; }
    public long? MarkedByUserId { get; set; }
    public string Status { get; set; } = "PRESENT";
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AttendanceSession? Session { get; set; }
    public Student? Student { get; set; }
}
