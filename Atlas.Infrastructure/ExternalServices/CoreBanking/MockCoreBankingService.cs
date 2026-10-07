namespace Atlas.Infrastructure.ExternalServices.CoreBanking;

using Atlas.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

/// <summary>
/// Mock implementation of core banking service for development and testing.
/// Provides idempotent account creation using ApplicationId as the key.
/// Implements requirements 6.1-6.5, 15.1.
/// </summary>
public class MockCoreBankingService : ICoreBankingService
{
    private readonly ILogger<MockCoreBankingService> _logger;
    private readonly ConcurrentDictionary<Guid, AccountCreationResult> _idempotencyCache;

    public MockCoreBankingService(ILogger<MockCoreBankingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _idempotencyCache = new ConcurrentDictionary<Guid, AccountCreationResult>();
    }

    /// <summary>
    /// Opens a new account with idempotent behavior based on ApplicationId.
    /// Multiple calls with the same ApplicationId return the same result.
    /// Implements requirement 15.1 (idempotency for external operations).
    /// </summary>
    public Task<AccountCreationResult> OpenAccountAsync(
        AccountCreationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Processing account creation request for ApplicationId: {ApplicationId}, Customer: {FirstName} {LastName}, Market: {MarketCode}",
            request.ApplicationId, request.FirstName, request.LastName, request.MarketCode);

        // Check idempotency cache - if this ApplicationId was already processed, return existing result
        if (_idempotencyCache.TryGetValue(request.ApplicationId, out var existingResult))
        {
            _logger.LogInformation(
                "Returning cached account creation result for ApplicationId: {ApplicationId}, AccountId: {AccountId}, AccountNumber: {AccountNumber}",
                request.ApplicationId, existingResult.AccountId, existingResult.AccountNumber);
            
            return Task.FromResult(existingResult);
        }

        // Generate new account
        var accountId = Guid.NewGuid();
        var accountNumber = $"ACC-{accountId:N}".Substring(0, 12); // "ACC-" + first 8 hex chars

        var result = new AccountCreationResult(
            AccountId: accountId,
            AccountNumber: accountNumber,
            CreatedAt: DateTime.UtcNow);

        // Store in idempotency cache
        _idempotencyCache.TryAdd(request.ApplicationId, result);

        _logger.LogInformation(
            "Account created successfully. ApplicationId: {ApplicationId}, AccountId: {AccountId}, AccountNumber: {AccountNumber}",
            request.ApplicationId, result.AccountId, result.AccountNumber);

        return Task.FromResult(result);
    }
}
