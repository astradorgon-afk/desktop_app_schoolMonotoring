namespace AttendanceUI.Data.Entities;

public class StudentExcuseRequest
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "PENDING";
    public long? ReviewedByUserId { get; set; }
    public string? ReviewNotes { get; set; }
    public byte[]? Attachment { get; set; }
    public string? AttachmentName { get; set; }
    public string? AttachmentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Student? Student { get; set; }
    public User? ReviewedBy { get; set; }
}
