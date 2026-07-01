namespace AttendanceUI.Models;

public class StudentListItem
{
    public long Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string Lrn { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Sex { get; set; }
    public int? Age { get; set; }
    public string? ContactNo { get; set; }
    public string? Address { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianContact { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? QrCodeBase64 { get; set; }
    public bool HasQrCode { get; set; }

    public string FullName => $"{LastName}, {FirstName}";
}
