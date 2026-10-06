namespace Atlas.Domain.ValueObjects;

/// <summary>
/// Represents the configuration for a specific market including national ID validation rules
/// and market-specific business rules.
/// </summary>
/// <param name="MarketCode">Two-letter market code (MA, MB, MC, MD, ME, MF).</param>
/// <param name="CountryName">Full name of the country/market.</param>
/// <param name="NationalIdRule">Validation rules for national ID format in this market.</param>
/// <param name="RequiresBranchActivation">Whether card orders in this market require branch activation (true for MD).</param>
/// <param name="IsActive">Whether this market is currently active for onboarding.</param>
public record MarketConfiguration(
    string MarketCode,
    string CountryName,
    NationalIdValidationRule NationalIdRule,
    bool RequiresBranchActivation,
    bool IsActive);
