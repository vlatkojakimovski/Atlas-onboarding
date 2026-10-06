namespace Atlas.Domain.Enums;

/// <summary>
/// Represents the result of sanctions and PEP screening.
/// </summary>
public enum ScreeningStatus
{
    /// <summary>
    /// No sanctions or PEP matches found; applicant is clear.
    /// </summary>
    Clear,

    /// <summary>
    /// Possible match found; requires review (auto-rejected in v1).
    /// </summary>
    PossibleMatch
}
