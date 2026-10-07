namespace Atlas.Infrastructure.Audit;

using Atlas.Domain.Enums;
using Atlas.Domain.ValueObjects;

/// <summary>
/// Defines the contract for audit logging throughout the application lifecycle.
/// Emits structured logs and persists audit events to the database for compliance.
/// Implements requirements 10.1-10.10, 18.1-18.8.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs the creation of a new application.
    /// Requirement 10.1: Audit trail must capture application submission.
    /// </summary>
    Task LogApplicationCreatedAsync(
        Guid applicationId,
        string marketCode,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs the completion of identity verification.
    /// Requirement 10.2: Audit trail must capture verification events.
    /// </summary>
    Task LogVerificationCompletedAsync(
        Guid applicationId,
        IdentityVerificationResult verificationResult,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs the completion of sanctions screening.
    /// Requirement 10.3: Audit trail must capture screening events.
    /// </summary>
    Task LogScreeningCompletedAsync(
        Guid applicationId,
        SanctionsScreeningResult screeningResult,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs the automated decision made on an application.
    /// Requirement 10.4: Audit trail must capture decision events.
    /// </summary>
    Task LogDecisionMadeAsync(
        Guid applicationId,
        ApplicationDecision decision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs the opening of a bank account.
    /// Requirement 10.5: Audit trail must capture account creation.
    /// </summary>
    Task LogAccountOpenedAsync(
        Guid applicationId,
        Guid accountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs the ordering of a debit card.
    /// Requirement 10.6: Audit trail must capture card ordering.
    /// </summary>
    Task LogCardOrderedAsync(
        Guid applicationId,
        Guid cardOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a change in application status.
    /// Requirement 10.7: Audit trail must capture status transitions.
    /// </summary>
    Task LogApplicationStatusChangedAsync(
        Guid applicationId,
        ApplicationStatus fromStatus,
        ApplicationStatus toStatus,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs data access events for compliance and security.
    /// Requirement 10.8: Audit trail must capture data access.
    /// </summary>
    Task LogDataAccessAsync(
        string userId,
        string resourceType,
        string resourceId,
        string action,
        CancellationToken cancellationToken = default);
}
