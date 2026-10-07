namespace Atlas.Infrastructure.ExternalServices.IdentityVerification;

using Atlas.Domain.ValueObjects;

/// <summary>
/// Defines the contract for identity document verification and face matching services.
/// In production, this would integrate with providers like IDNow, Onfido, or Jumio.
/// Implements requirements 3.1-3.6.
/// </summary>
public interface IIdentityVerificationService
{
    /// <summary>
    /// Verifies the authenticity of identity documents and performs face matching.
    /// </summary>
    /// <param name="request">The verification request containing document images and selfie.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>A verification result with document status, face match result, and confidence score.</returns>
    Task<IdentityVerificationResult> VerifyIdentityAsync(
        IdentityVerificationRequest request,
        CancellationToken cancellationToken = default);
}
