namespace Atlas.Domain.ValueObjects;

/// <summary>
/// Represents the result of account creation in the core banking system.
/// </summary>
/// <param name="AccountId">Unique identifier for the created account.</param>
/// <param name="AccountNumber">Human-readable account number.</param>
/// <param name="CreatedAt">UTC timestamp when the account was created.</param>
public record AccountCreationResult(
    Guid AccountId,
    string AccountNumber,
    DateTime CreatedAt);
