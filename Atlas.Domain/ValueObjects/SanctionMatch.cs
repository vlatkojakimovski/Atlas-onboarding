namespace Atlas.Domain.ValueObjects;

/// <summary>
/// Represents a single sanctions or PEP match found during screening.
/// </summary>
/// <param name="Type">Type of match (Sanctions or PEP).</param>
/// <param name="Score">Match confidence score (0.0 to 1.0).</param>
/// <param name="Subject">Description of the matched subject/entity.</param>
public record SanctionMatch(
    Enums.MatchType Type,
    decimal Score,
    string Subject);
