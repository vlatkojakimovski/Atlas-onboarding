namespace Atlas.Infrastructure.ExternalServices.CoreBanking;

using Atlas.Domain.ValueObjects;

/// <summary>
/// Defines the contract for core banking system integration.
/// In production, this would integrate with the bank's core banking platform.
/// This is a seam for future implementation.
/// Implements requirements 6.1-6.5, 15.1.
/// </summary>
public interface ICoreBankingService
{
    /// <summary>
    /// Opens a new account in the core banking system for an approved application.
    /// </summary>
    /// <param name="request">The account creation request containing customer details.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Account creation result with account ID and account number.</returns>
    Task<AccountCreationResult> OpenAccountAsync(
        AccountCreationRequest request,
        CancellationToken cancellationToken = default);
}
