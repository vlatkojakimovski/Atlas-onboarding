namespace Atlas.Domain.Enums;

/// <summary>
/// Represents the type of identity document submitted by the applicant.
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Passport document.
    /// </summary>
    PASSPORT,

    /// <summary>
    /// National identity card.
    /// </summary>
    ID_CARD,

    /// <summary>
    /// Selfie photograph for face matching.
    /// </summary>
    SELFIE
}
