namespace Atlas.Domain.ValueObjects;

/// <summary>
/// Represents the result of a card order in the card issuing system.
/// </summary>
/// <param name="CardOrderId">Unique identifier for the card order.</param>
/// <param name="CardReference">Card reference number for tracking.</param>
/// <param name="OrderedAt">UTC timestamp when the card was ordered.</param>
public record CardOrderResult(
    Guid CardOrderId,
    string CardReference,
    DateTime OrderedAt);
