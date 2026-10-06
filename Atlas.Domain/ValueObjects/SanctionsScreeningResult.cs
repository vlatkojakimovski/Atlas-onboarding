using Atlas.Domain.Enums;

namespace Atlas.Domain.ValueObjects;

/// <summary>
/// Represents the result of sanctions and PEP screening from WorldCheck provider.
/// </summary>
/// <param name="CaseId">Unique case identifier from the screening provider (e.g., WorldCheck case ID).</param>
/// <param name="Status">Overall screening status (Clear or PossibleMatch).</param>
/// <param name="Matches">List of potential matches found (empty if Status is Clear).</param>
/// <param name="ScreenedAt">UTC timestamp when screening was completed.</param>
public record SanctionsScreeningResult(
    string CaseId,
    ScreeningStatus Status,
    IReadOnlyList<SanctionMatch> Matches,
    DateTime ScreenedAt);
