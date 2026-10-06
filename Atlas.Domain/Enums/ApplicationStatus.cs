namespace Atlas.Domain.Enums;

/// <summary>
/// Represents the current status of a customer onboarding application.
/// </summary>
public enum ApplicationStatus
{
    /// <summary>
    /// Application has been submitted and is awaiting processing.
    /// </summary>
    Pending,

    /// <summary>
    /// Application has been approved; account and card have been created.
    /// </summary>
    Approved,

    /// <summary>
    /// Application has been rejected due to verification or screening failures.
    /// </summary>
    Rejected,

    /// <summary>
    /// Application is pending manual review (unused in v1, reserved for future workflow).
    /// </summary>
    PendingReview
}
