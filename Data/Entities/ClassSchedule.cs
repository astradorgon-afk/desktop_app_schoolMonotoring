namespace AttendanceUI.Data.Entities;

public class ClassSchedule
{
    public long Id { get; set; }
    public long ClassOfferingId { get; set; }
    public long? RoomId { get; set; }
    public long? TimeSlotId { get; set; }
    public byte DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ClassOffering? ClassOffering { get; set; }
}
