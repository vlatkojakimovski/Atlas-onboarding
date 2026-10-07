namespace Atlas.Domain.BusinessLogic;

using Atlas.Domain.ValueObjects;

/// <summary>
/// Defines the contract for making automated decisions on customer onboarding applications
/// based on identity verification and sanctions screening results.
/// </summary>
public interface IApplicationDecisionEngine
{
    /// <summary>
    /// Makes a deterministic decision on an application based on verification and screening results.
    /// This is a pure function - same inputs always produce same outputs.
    /// Implements requirements 3.3-3.6, 4.3-4.6, 5.1-5.7, 19.2.
    /// </summary>
    /// <param name="verificationResult">Result from identity verification provider.</param>
    /// <param name="screeningResult">Result from sanctions screening provider.</param>
    /// <param name="marketConfiguration">Market-specific configuration for the application.</param>
    /// <returns>An application decision with status and rejection reasons (if applicable).</returns>
    ApplicationDecision MakeDecision(
        IdentityVerificationResult verificationResult,
        SanctionsScreeningResult screeningResult,
        MarketConfiguration marketConfiguration);
}
