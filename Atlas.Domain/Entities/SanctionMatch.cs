using Atlas.Domain.Enums;

namespace Atlas.Domain.Entities;

/// <summary>
/// Represents a single sanction or PEP match found during screening.
/// </summary>
public class SanctionMatch
{
    private SanctionMatch() { } // For EF Core

    public SanctionMatch(Enums.MatchType type, decimal score, string subject)
    {
        Id = Guid.NewGuid();
        Type = type;
        Score = score;
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
    }

    public Guid Id { get; private set; }
    public Guid ScreeningId { get; private set; }
    public Enums.MatchType Type { get; private set; }
    public decimal Score { get; private set; }
    public string Subject { get; private set; } = string.Empty;
}
