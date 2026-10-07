namespace Atlas.Infrastructure.ExternalServices.CoreBanking;

/// <summary>
/// Request model for creating an account in the core banking system.
/// </summary>
/// <param name="ApplicationId">The application ID (used as idempotency key).</param>
/// <param name="FirstName">Customer's first name.</param>
/// <param name="LastName">Customer's last name.</param>
/// <param name="DateOfBirth">Customer's date of birth.</param>
/// <param name="NationalId">Customer's national ID.</param>
/// <param name="Email">Customer's email address.</param>
/// <param name="Phone">Customer's phone number.</param>
/// <param name="MarketCode">Market code (MA, MB, MC, MD, ME, MF).</param>
public record AccountCreationRequest(
    Guid ApplicationId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string NationalId,
    string Email,
    string Phone,
    string MarketCode);
