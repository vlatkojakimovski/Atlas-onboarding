namespace Atlas.Infrastructure.Data.Entities;

public class SanctionsScreeningEntity
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string CaseId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Clear, PossibleMatch
    public DateTime ScreenedAt { get; set; }

    // Navigation properties
    public ApplicationEntity Application { get; set; } = null!;
    public ICollection<SanctionMatchEntity> Matches { get; set; } = new List<SanctionMatchEntity>();
}
