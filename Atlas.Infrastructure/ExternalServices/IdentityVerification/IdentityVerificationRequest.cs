namespace Atlas.Infrastructure.ExternalServices.IdentityVerification;

using Atlas.Domain.Enums;

/// <summary>
/// Request model for identity verification service containing document images.
/// </summary>
/// <param name="DocumentType">Type of identity document (PASSPORT or ID_CARD).</param>
/// <param name="DocumentImageBase64">Base64-encoded image of the identity document.</param>
/// <param name="SelfieImageBase64">Base64-encoded selfie image for face matching.</param>
public record IdentityVerificationRequest(
    DocumentType DocumentType,
    string DocumentImageBase64,
    string SelfieImageBase64);
