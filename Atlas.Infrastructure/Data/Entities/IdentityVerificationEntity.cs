namespace Atlas.Infrastructure.Data.Entities;

public class IdentityVerificationEntity
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string ProviderId { get; set; } = string.Empty;
    public string DocumentStatus { get; set; } = string.Empty; // Valid, Invalid, Inconclusive
    public bool FaceMatch { get; set; }
    public decimal Confidence { get; set; }
    public DateTime VerifiedAt { get; set; }

    // Navigation properties
    public ApplicationEntity Application { get; set; } = null!;
}
