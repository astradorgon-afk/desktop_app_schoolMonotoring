namespace AttendanceUI.Data.Entities;

public class StudentQrCode
{
    public int Id { get; set; }
    public long StudentId { get; set; }
    public string QrContent { get; set; } = string.Empty;
    public byte[] QrCode { get; set; } = [];
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
