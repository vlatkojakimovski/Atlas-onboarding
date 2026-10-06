namespace Atlas.Infrastructure.Data.Entities;

public class CardOrderEntity
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid AccountId { get; set; }
    public Guid CardOrderId { get; set; }
    public string CardReference { get; set; } = string.Empty;
    public bool RequiresBranchActivation { get; set; }
    public DateTime OrderedAt { get; set; }

    // Navigation properties
    public ApplicationEntity Application { get; set; } = null!;
}
