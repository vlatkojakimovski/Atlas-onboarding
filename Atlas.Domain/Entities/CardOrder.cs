namespace Atlas.Domain.Entities;

/// <summary>
/// Represents a card order for an approved application.
/// </summary>
public class CardOrder
{
    private CardOrder() { } // For EF Core

    public CardOrder(
        Guid applicationId,
        Guid accountId,
        Guid cardOrderId,
        string cardReference,
        bool requiresBranchActivation,
        DateTime orderedAt)
    {
        Id = Guid.NewGuid();
        ApplicationId = applicationId;
        AccountId = accountId;
        CardOrderId = cardOrderId;
        CardReference = cardReference ?? throw new ArgumentNullException(nameof(cardReference));
        RequiresBranchActivation = requiresBranchActivation;
        OrderedAt = orderedAt;
    }

    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid CardOrderId { get; private set; }
    public string CardReference { get; private set; } = string.Empty;
    public bool RequiresBranchActivation { get; private set; }
    public DateTime OrderedAt { get; private set; }
}
