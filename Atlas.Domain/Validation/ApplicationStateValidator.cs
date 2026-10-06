using Atlas.Domain.Enums;

namespace Atlas.Domain.Validation;

/// <summary>
/// Validates application state transitions to enforce business rules.
/// </summary>
public static class ApplicationStateValidator
{
    /// <summary>
    /// Validates whether a state transition is allowed.
    /// </summary>
    /// <param name="currentStatus">The current application status.</param>
    /// <param name="newStatus">The desired new status.</param>
    /// <returns>True if the transition is valid; otherwise, false.</returns>
    public static bool IsValidTransition(ApplicationStatus currentStatus, ApplicationStatus newStatus)
    {
        // Can only transition from Pending to Approved or Rejected
        // No reversals allowed (immutable once decided)
        return currentStatus switch
        {
            ApplicationStatus.Pending => newStatus is ApplicationStatus.Approved 
                                          or ApplicationStatus.Rejected 
                                          or ApplicationStatus.PendingReview,
            
            ApplicationStatus.PendingReview => newStatus is ApplicationStatus.Approved 
                                                or ApplicationStatus.Rejected,
            
            // Terminal states - no transitions allowed
            ApplicationStatus.Approved => false,
            ApplicationStatus.Rejected => false,
            
            _ => false
        };
    }

    /// <summary>
    /// Gets the validation error message for an invalid state transition.
    /// </summary>
    /// <param name="currentStatus">The current application status.</param>
    /// <param name="newStatus">The desired new status.</param>
    /// <returns>Error message describing why the transition is invalid.</returns>
    public static string GetTransitionErrorMessage(ApplicationStatus currentStatus, ApplicationStatus newStatus)
    {
        return currentStatus switch
        {
            ApplicationStatus.Approved => $"Cannot transition from {currentStatus} to {newStatus}. Approved applications are immutable.",
            ApplicationStatus.Rejected => $"Cannot transition from {currentStatus} to {newStatus}. Rejected applications are immutable.",
            ApplicationStatus.Pending => $"Cannot transition from {currentStatus} to {newStatus}. Only Approved, Rejected, or PendingReview are valid.",
            ApplicationStatus.PendingReview => $"Cannot transition from {currentStatus} to {newStatus}. Only Approved or Rejected are valid.",
            _ => $"Invalid state transition from {currentStatus} to {newStatus}."
        };
    }

    /// <summary>
    /// Determines if an application status is a terminal state (cannot be changed).
    /// </summary>
    /// <param name="status">The application status to check.</param>
    /// <returns>True if the status is terminal; otherwise, false.</returns>
    public static bool IsTerminalState(ApplicationStatus status)
    {
        return status is ApplicationStatus.Approved or ApplicationStatus.Rejected;
    }

    /// <summary>
    /// Determines if an application status allows modifications (adding documents, etc.).
    /// </summary>
    /// <param name="status">The application status to check.</param>
    /// <returns>True if modifications are allowed; otherwise, false.</returns>
    public static bool AllowsModifications(ApplicationStatus status)
    {
        return status is ApplicationStatus.Pending;
    }
}
