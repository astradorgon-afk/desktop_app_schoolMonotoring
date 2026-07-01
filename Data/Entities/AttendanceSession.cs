namespace AttendanceUI.Data.Entities;

public class AttendanceSession
{
    public long Id { get; set; }
    public long ClassOfferingId { get; set; }
    public DateTime SessionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ClassOffering? ClassOffering { get; set; }
    public ICollection<AttendanceRecord> Records { get; set; } = [];
}
