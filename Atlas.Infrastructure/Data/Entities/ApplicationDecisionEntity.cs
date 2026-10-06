namespace Atlas.Infrastructure.Data.Entities;

public class ApplicationDecisionEntity
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DecidedAt { get; set; }

    // Navigation properties
    public ApplicationEntity Application { get; set; } = null!;
    public ICollection<RejectionReasonEntity> RejectionReasons { get; set; } = new List<RejectionReasonEntity>();
}
