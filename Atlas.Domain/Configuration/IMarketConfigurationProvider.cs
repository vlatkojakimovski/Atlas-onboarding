namespace Atlas.Domain.Configuration;

using Atlas.Domain.ValueObjects;

/// <summary>
/// Provides access to market-specific configuration for application processing.
/// </summary>
public interface IMarketConfigurationProvider
{
    /// <summary>
    /// Retrieves the configuration for a specific market.
    /// </summary>
    /// <param name="marketCode">Two-letter market code (MA, MB, MC, MD, ME, MF).</param>
    /// <returns>The market configuration if found; otherwise, null.</returns>
    MarketConfiguration? GetMarketConfiguration(string marketCode);

    /// <summary>
    /// Gets all configured markets.
    /// </summary>
    /// <returns>A read-only collection of all market configurations.</returns>
    IReadOnlyCollection<MarketConfiguration> GetAllMarkets();

    /// <summary>
    /// Checks if a market is configured and active.
    /// </summary>
    /// <param name="marketCode">Two-letter market code to validate.</param>
    /// <returns>True if the market is configured and active; otherwise, false.</returns>
    bool IsMarketActive(string marketCode);
}
