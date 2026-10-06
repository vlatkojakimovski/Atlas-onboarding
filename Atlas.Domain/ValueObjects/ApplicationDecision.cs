using Atlas.Domain.Enums;

namespace Atlas.Domain.ValueObjects;

/// <summary>
/// Represents the decision made on an application (approved or rejected with reasons).
/// </summary>
/// <param name="Status">The application status after decision (Approved, Rejected, or PendingReview).</param>
/// <param name="Reasons">List of rejection reasons (empty if approved).</param>
public record ApplicationDecision(
    ApplicationStatus Status,
    IReadOnlyList<RejectionReason> Reasons)
{
    /// <summary>
    /// Gets whether the application was approved.
    /// </summary>
    public bool IsApproved => Status == ApplicationStatus.Approved;

    /// <summary>
    /// Gets whether the application was rejected.
    /// </summary>
    public bool IsRejected => Status == ApplicationStatus.Rejected;
}
