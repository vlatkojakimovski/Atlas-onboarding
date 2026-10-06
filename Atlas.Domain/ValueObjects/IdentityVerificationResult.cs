using Atlas.Domain.Enums;

namespace Atlas.Domain.ValueObjects;

/// <summary>
/// Represents the result of identity document verification and face matching from IDNow provider.
/// </summary>
/// <param name="ProviderId">Unique identifier from the identity verification provider (e.g., IDNow reference).</param>
/// <param name="DocumentStatus">Result of document authenticity verification.</param>
/// <param name="FaceMatch">Whether the face in the selfie matches the face in the identity document.</param>
/// <param name="Confidence">Confidence score of the verification (0.0 to 1.0).</param>
/// <param name="VerifiedAt">UTC timestamp when verification was completed.</param>
public record IdentityVerificationResult(
    string ProviderId,
    DocumentVerificationStatus DocumentStatus,
    bool FaceMatch,
    decimal Confidence,
    DateTime VerifiedAt);
