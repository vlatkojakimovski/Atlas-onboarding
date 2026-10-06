namespace Atlas.Infrastructure.Data.Entities;

public class SanctionMatchEntity
{
    public Guid Id { get; set; }
    public Guid ScreeningId { get; set; }
    public string MatchType { get; set; } = string.Empty; // Sanctions, PEP
    public decimal Score { get; set; }
    public string Subject { get; set; } = string.Empty;

    // Navigation properties
    public SanctionsScreeningEntity Screening { get; set; } = null!;
}
