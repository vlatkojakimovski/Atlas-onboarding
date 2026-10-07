namespace Atlas.Domain.BusinessLogic;

using Atlas.Domain.Enums;
using Atlas.Domain.ValueObjects;

/// <summary>
/// Implements automated decision-making logic for customer onboarding applications.
/// This is a pure function with no external dependencies - deterministic and testable.
/// Implements requirements 3.3-3.6, 4.3-4.6, 5.1-5.7, 19.2.
/// </summary>
public class ApplicationDecisionEngine : IApplicationDecisionEngine
{
    /// <summary>
    /// Makes a deterministic decision on an application based on verification and screening results.
    /// </summary>
    /// <param name="verificationResult">Result from identity verification provider.</param>
    /// <param name="screeningResult">Result from sanctions screening provider.</param>
    /// <param name="marketConfiguration">Market-specific configuration for the application.</param>
    /// <returns>An application decision with status and rejection reasons (if applicable).</returns>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public ApplicationDecision MakeDecision(
        IdentityVerificationResult verificationResult,
        SanctionsScreeningResult screeningResult,
        MarketConfiguration marketConfiguration)
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(verificationResult);
        ArgumentNullException.ThrowIfNull(screeningResult);
        ArgumentNullException.ThrowIfNull(marketConfiguration);

        // Accumulate rejection reasons
        var rejectionReasons = new List<RejectionReason>();

        // Check if providers were unavailable (indicated by null or empty provider IDs)
        // This handles the case where external services failed after all retries
        if (string.IsNullOrWhiteSpace(verificationResult.ProviderId))
        {
            rejectionReasons.Add(RejectionReason.VerificationProviderUnavailable);
        }

        if (string.IsNullOrWhiteSpace(screeningResult.CaseId))
        {
            rejectionReasons.Add(RejectionReason.ScreeningProviderUnavailable);
        }

        // If providers were unavailable, immediately reject
        if (rejectionReasons.Any())
        {
            return new ApplicationDecision(
                Status: ApplicationStatus.Rejected,
                Reasons: rejectionReasons.AsReadOnly());
        }

        // Evaluate identity verification results
        // Requirement 3.4: Invalid document → REJECTED
        if (verificationResult.DocumentStatus == DocumentVerificationStatus.Invalid)
        {
            rejectionReasons.Add(RejectionReason.InvalidDocument);
        }

        // Requirement 3.5: Inconclusive document → REJECTED
        if (verificationResult.DocumentStatus == DocumentVerificationStatus.Inconclusive)
        {
            rejectionReasons.Add(RejectionReason.InconclusiveVerificationResult);
        }

        // Requirement 3.6: Face mismatch → REJECTED
        if (!verificationResult.FaceMatch)
        {
            rejectionReasons.Add(RejectionReason.FaceMismatch);
        }

        // Evaluate sanctions screening results
        // Requirement 4.4 & 19.2: PossibleMatch → REJECTED in v1 (auto-rejection, no manual review)
        if (screeningResult.Status == ScreeningStatus.PossibleMatch)
        {
            rejectionReasons.Add(RejectionReason.PossibleSanctionMatch);
        }

        // Additional check: if there are actual matches with Sanctions type
        // Requirement 4.5: Clear sanctions match → REJECTED
        if (screeningResult.Matches != null && screeningResult.Matches.Any(m => m.Type == MatchType.Sanctions))
        {
            // If we already added PossibleSanctionMatch, don't duplicate - but ensure SanctionMatch is present
            if (!rejectionReasons.Contains(RejectionReason.PossibleSanctionMatch))
            {
                rejectionReasons.Add(RejectionReason.SanctionMatch);
            }
        }

        // If any rejection reasons accumulated, return REJECTED
        if (rejectionReasons.Any())
        {
            return new ApplicationDecision(
                Status: ApplicationStatus.Rejected,
                Reasons: rejectionReasons.AsReadOnly());
        }

        // Requirement 3.3 & 5.1: Approval rule
        // DocumentStatus == Valid AND FaceMatch == true AND ScreeningStatus == Clear → APPROVED
        if (verificationResult.DocumentStatus == DocumentVerificationStatus.Valid &&
            verificationResult.FaceMatch &&
            screeningResult.Status == ScreeningStatus.Clear)
        {
            return new ApplicationDecision(
                Status: ApplicationStatus.Approved,
                Reasons: Array.Empty<RejectionReason>());
        }

        // Fallback: If we reach here, something unexpected happened
        // This should not occur in normal operation, but provides safety
        rejectionReasons.Add(RejectionReason.InconclusiveVerificationResult);
        return new ApplicationDecision(
            Status: ApplicationStatus.Rejected,
            Reasons: rejectionReasons.AsReadOnly());
    }
}
