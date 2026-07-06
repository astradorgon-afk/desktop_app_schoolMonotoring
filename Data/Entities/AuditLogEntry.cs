namespace AttendanceUI.Data.Entities;

public class AuditLogEntry
{
    public long Id { get; set; }
    public long ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
