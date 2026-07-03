namespace AttendanceUI.Data.Entities;

public enum QrTokenModelType
{
    ROOM,
    STUDENT
}

public class QrToken
{
    public long Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public QrTokenModelType ModelType { get; set; }
    public long ModelId { get; set; }
    public string? BorrowPurpose { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
