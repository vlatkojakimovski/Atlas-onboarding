using Atlas.Domain.Enums;

namespace Atlas.Domain.Entities;

/// <summary>
/// Represents the result of sanctions screening for an application.
/// </summary>
public class SanctionsScreening
{
    private SanctionsScreening() 
    { 
        Matches = new List<SanctionMatch>();
    } // For EF Core

    public SanctionsScreening(
        Guid applicationId,
        string caseId,
        ScreeningStatus status,
        DateTime screenedAt,
        List<SanctionMatch> matches)
    {
        Id = Guid.NewGuid();
        ApplicationId = applicationId;
        CaseId = caseId ?? throw new ArgumentNullException(nameof(caseId));
        Status = status;
        ScreenedAt = screenedAt;
        Matches = matches ?? new List<SanctionMatch>();
    }

    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string CaseId { get; private set; } = string.Empty;
    public ScreeningStatus Status { get; private set; }
    public DateTime ScreenedAt { get; private set; }
    public ICollection<SanctionMatch> Matches { get; private set; }
}
