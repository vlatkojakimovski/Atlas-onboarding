namespace Atlas.Domain.Enums;

/// <summary>
/// Represents the type of sanctions/PEP match found during screening.
/// </summary>
public enum MatchType
{
    /// <summary>
    /// Match against sanctions list (OFAC, UN, EU, etc.).
    /// </summary>
    Sanctions,

    /// <summary>
    /// Match against Politically Exposed Persons (PEP) list.
    /// </summary>
    PEP
}
