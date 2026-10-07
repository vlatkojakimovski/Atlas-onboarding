namespace Atlas.Application.Models;

using Atlas.Domain.Enums;

/// <summary>
/// Represents the result of application submission and processing.
/// </summary>
/// <param name="ApplicationId">The unique identifier of the application.</param>
/// <param name="Status">The current status of the application.</param>
/// <param name="RejectionReasons">List of rejection reasons if status is Rejected (empty otherwise).</param>
/// <param name="AccountId">The account ID if approved (null otherwise).</param>
/// <param name="CardOrderId">The card order ID if approved (null otherwise).</param>
public record ApplicationResult(
    Guid ApplicationId,
    ApplicationStatus Status,
    IReadOnlyList<RejectionReason> RejectionReasons,
    Guid? AccountId,
    Guid? CardOrderId);
