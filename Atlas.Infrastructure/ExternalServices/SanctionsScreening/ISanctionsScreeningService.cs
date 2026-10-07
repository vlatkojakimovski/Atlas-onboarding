namespace Atlas.Infrastructure.ExternalServices.SanctionsScreening;

using Atlas.Domain.ValueObjects;

/// <summary>
/// Defines the contract for sanctions and PEP screening services.
/// In production, this would integrate with providers like WorldCheck, Dow Jones, or ComplyAdvantage.
/// Implements requirements 4.1-4.6.
/// </summary>
public interface ISanctionsScreeningService
{
    /// <summary>
    /// Screens an applicant against sanctions lists and PEP databases.
    /// </summary>
    /// <param name="request">The screening request containing applicant details.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>A screening result with status and any matches found.</returns>
    Task<SanctionsScreeningResult> ScreenAsync(
        SanctionsScreeningRequest request,
        CancellationToken cancellationToken = default);
}
