namespace Atlas.Domain.Enums;

/// <summary>
/// Represents the result of document authenticity verification by the identity provider.
/// </summary>
public enum DocumentVerificationStatus
{
    /// <summary>
    /// Document is authentic and valid.
    /// </summary>
    Valid,

    /// <summary>
    /// Document is fraudulent, expired, or invalid.
    /// </summary>
    Invalid,

    /// <summary>
    /// Document authenticity could not be determined conclusively.
    /// </summary>
    Inconclusive
}
