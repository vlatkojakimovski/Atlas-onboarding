namespace Atlas.Infrastructure.Data.Entities;

public class AuditLogEntity
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public Guid? ApplicationId { get; set; }
    public string? MarketCode { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? Action { get; set; }
    public string? Details { get; set; }

    // Navigation properties
    public ApplicationEntity? Application { get; set; }
}
