namespace Atlas.Domain.Enums;

/// <summary>
/// Represents the specific reason(s) why an application was rejected.
/// Multiple reasons may apply to a single application.
/// </summary>
public enum RejectionReason
{
    /// <summary>
    /// Identity document is invalid, fraudulent, or expired.
    /// </summary>
    InvalidDocument,

    /// <summary>
    /// Face in selfie does not match the face in identity document.
    /// </summary>
    FaceMismatch,

    /// <summary>
    /// Identity verification result was inconclusive.
    /// </summary>
    InconclusiveVerificationResult,

    /// <summary>
    /// Applicant matched a sanctions list (confirmed match).
    /// </summary>
    SanctionMatch,

    /// <summary>
    /// Applicant has a possible match to sanctions/PEP list (auto-rejected in v1).
    /// </summary>
    PossibleSanctionMatch,

    /// <summary>
    /// Identity verification provider was unavailable after retries.
    /// </summary>
    VerificationProviderUnavailable,

    /// <summary>
    /// Sanctions screening provider was unavailable after retries.
    /// </summary>
    ScreeningProviderUnavailable
}
