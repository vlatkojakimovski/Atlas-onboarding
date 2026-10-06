using Atlas.Domain.Enums;

namespace Atlas.Domain.Entities;

/// <summary>
/// Represents the result of identity verification for an application.
/// </summary>
public class IdentityVerification
{
    private IdentityVerification() { } // For EF Core

    public IdentityVerification(
        Guid applicationId,
        string providerId,
        DocumentVerificationStatus documentStatus,
        bool faceMatch,
        decimal confidence,
        DateTime verifiedAt)
    {
        Id = Guid.NewGuid();
        ApplicationId = applicationId;
        ProviderId = providerId ?? throw new ArgumentNullException(nameof(providerId));
        DocumentStatus = documentStatus;
        FaceMatch = faceMatch;
        Confidence = confidence;
        VerifiedAt = verifiedAt;
    }

    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string ProviderId { get; private set; } = string.Empty;
    public DocumentVerificationStatus DocumentStatus { get; private set; }
    public bool FaceMatch { get; private set; }
    public decimal Confidence { get; private set; }
    public DateTime VerifiedAt { get; private set; }
}
