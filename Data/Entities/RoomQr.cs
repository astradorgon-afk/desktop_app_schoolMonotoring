namespace AttendanceUI.Data.Entities;

public class RoomQr
{
    public long Id { get; set; }
    public long RoomId { get; set; }
    public string QrContent { get; set; } = string.Empty;
    public byte[] QrCode { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Room? Room { get; set; }
}
