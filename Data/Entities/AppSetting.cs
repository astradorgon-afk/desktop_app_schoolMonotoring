namespace AttendanceUI.Data.Entities;

public class AppSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public long? UpdatedByUserId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
