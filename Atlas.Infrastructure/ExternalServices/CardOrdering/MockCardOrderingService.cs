namespace Atlas.Infrastructure.ExternalServices.CardOrdering;

using Atlas.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

/// <summary>
/// Mock implementation of card ordering service for development and testing.
/// Provides idempotent card ordering using AccountId as the key.
/// Implements requirements 7.1-7.6, 15.2.
/// </summary>
public class MockCardOrderingService : ICardOrderingService
{
    private readonly ILogger<MockCardOrderingService> _logger;
    private readonly ConcurrentDictionary<Guid, CardOrderResult> _idempotencyCache;

    public MockCardOrderingService(ILogger<MockCardOrderingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _idempotencyCache = new ConcurrentDictionary<Guid, CardOrderResult>();
    }

    /// <summary>
    /// Orders a card with idempotent behavior based on AccountId.
    /// Multiple calls with the same AccountId return the same result.
    /// Implements requirement 15.2 (idempotency for external operations).
    /// </summary>
    public Task<CardOrderResult> OrderCardAsync(
        CardOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Processing card order request for AccountId: {AccountId}, Cardholder: {FirstName} {LastName}, RequiresBranchActivation: {RequiresBranchActivation}",
            request.AccountId, request.FirstName, request.LastName, request.RequiresBranchActivation);

        // Check idempotency cache - if this AccountId was already processed, return existing result
        if (_idempotencyCache.TryGetValue(request.AccountId, out var existingResult))
        {
            _logger.LogInformation(
                "Returning cached card order result for AccountId: {AccountId}, CardOrderId: {CardOrderId}, CardReference: {CardReference}",
                request.AccountId, existingResult.CardOrderId, existingResult.CardReference);
            
            return Task.FromResult(existingResult);
        }

        // Generate new card order
        var cardOrderId = Guid.NewGuid();
        var cardReference = $"CARD-{cardOrderId:N}".Substring(0, 13); // "CARD-" + first 8 hex chars

        var result = new CardOrderResult(
            CardOrderId: cardOrderId,
            CardReference: cardReference,
            OrderedAt: DateTime.UtcNow);

        // Store in idempotency cache
        _idempotencyCache.TryAdd(request.AccountId, result);

        _logger.LogInformation(
            "Card ordered successfully. AccountId: {AccountId}, CardOrderId: {CardOrderId}, CardReference: {CardReference}, RequiresBranchActivation: {RequiresBranchActivation}",
            request.AccountId, result.CardOrderId, result.CardReference, request.RequiresBranchActivation);

        if (request.RequiresBranchActivation)
        {
            _logger.LogInformation(
                "Card for AccountId: {AccountId} requires branch activation (MD market)",
                request.AccountId);
        }

        return Task.FromResult(result);
    }
}
