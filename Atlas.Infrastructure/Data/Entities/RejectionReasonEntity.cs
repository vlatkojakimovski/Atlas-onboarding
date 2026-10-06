namespace Atlas.Infrastructure.Data.Entities;

public class RejectionReasonEntity
{
    public Guid Id { get; set; }
    public Guid DecisionId { get; set; }
    public string Reason { get; set; } = string.Empty;

    // Navigation properties
    public ApplicationDecisionEntity Decision { get; set; } = null!;
}
