namespace Atlas.Infrastructure.ExternalServices.CardOrdering;

using Atlas.Domain.ValueObjects;

/// <summary>
/// Defines the contract for card ordering system integration.
/// In production, this would integrate with the bank's card management system.
/// This is a seam for future implementation.
/// Implements requirements 7.1-7.6, 15.2.
/// </summary>
public interface ICardOrderingService
{
    /// <summary>
    /// Orders a debit card for a newly opened account.
    /// </summary>
    /// <param name="request">The card order request containing account and customer details.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Card order result with order ID and card reference.</returns>
    Task<CardOrderResult> OrderCardAsync(
        CardOrderRequest request,
        CancellationToken cancellationToken = default);
}
