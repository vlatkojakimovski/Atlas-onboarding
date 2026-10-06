namespace Atlas.Infrastructure.Data.Entities;

public class AccountDetailsEntity
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ApplicationEntity Application { get; set; } = null!;
}
