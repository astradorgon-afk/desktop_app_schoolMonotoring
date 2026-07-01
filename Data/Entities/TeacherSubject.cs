namespace AttendanceUI.Data.Entities;

public class TeacherSubject
{
    public int IdteacherSubject { get; set; }
    public long TeacherId { get; set; }
    public string SubjectTitle { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Teacher? Teacher { get; set; }
}
