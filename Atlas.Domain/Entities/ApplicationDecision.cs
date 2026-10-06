using Atlas.Domain.Enums;

namespace Atlas.Domain.Entities;

/// <summary>
/// Represents the decision made on an application (approved or rejected with reasons).
/// </summary>
public class ApplicationDecision
{
    private ApplicationDecision() 
    { 
        RejectionReasons = new List<RejectionReason>();
    } // For EF Core

    public ApplicationDecision(
        Guid applicationId,
        ApplicationStatus status,
        DateTime decidedAt,
        List<RejectionReason> rejectionReasons)
    {
        Id = Guid.NewGuid();
        ApplicationId = applicationId;
        Status = status;
        DecidedAt = decidedAt;
        RejectionReasons = rejectionReasons ?? new List<RejectionReason>();
    }

    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public ApplicationStatus Status { get; private set; }
    public DateTime DecidedAt { get; private set; }
    public ICollection<RejectionReason> RejectionReasons { get; private set; }
}
