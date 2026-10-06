namespace Atlas.Domain.Entities;

/// <summary>
/// Represents the account details created for an approved application.
/// </summary>
public class AccountDetails
{
    private AccountDetails() { } // For EF Core

    public AccountDetails(Guid applicationId, Guid accountId, string accountNumber, DateTime createdAt)
    {
        Id = Guid.NewGuid();
        ApplicationId = applicationId;
        AccountId = accountId;
        AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid AccountId { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
}
