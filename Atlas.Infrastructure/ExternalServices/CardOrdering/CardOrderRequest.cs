namespace Atlas.Infrastructure.ExternalServices.CardOrdering;

/// <summary>
/// Request model for ordering a card in the card management system.
/// </summary>
/// <param name="AccountId">The account ID for which the card is being ordered (used as idempotency key).</param>
/// <param name="FirstName">Cardholder's first name.</param>
/// <param name="LastName">Cardholder's last name.</param>
/// <param name="DeliveryAddress">Address for card delivery.</param>
/// <param name="RequiresBranchActivation">Whether the card requires branch activation (true for MD market).</param>
public record CardOrderRequest(
    Guid AccountId,
    string FirstName,
    string LastName,
    string DeliveryAddress,
    bool RequiresBranchActivation);
