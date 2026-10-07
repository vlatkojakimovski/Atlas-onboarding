namespace Atlas.Infrastructure.Configuration;

using Atlas.Domain.Configuration;
using Atlas.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

/// <summary>
/// Provides market-specific configuration loaded from appsettings.json with caching and hot-reload support.
/// Implements requirements 14.4-14.7 and 16.1-16.5.
/// </summary>
public class MarketConfigurationProvider : IMarketConfigurationProvider
{
    private readonly IOptionsMonitor<MarketsConfiguration> _marketsConfigMonitor;
    private readonly ILogger<MarketConfigurationProvider> _logger;
    private readonly ConcurrentDictionary<string, MarketConfiguration> _cache;

    public MarketConfigurationProvider(
        IOptionsMonitor<MarketsConfiguration> marketsConfigMonitor,
        ILogger<MarketConfigurationProvider> logger)
    {
        _marketsConfigMonitor = marketsConfigMonitor ?? throw new ArgumentNullException(nameof(marketsConfigMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = new ConcurrentDictionary<string, MarketConfiguration>(StringComparer.OrdinalIgnoreCase);

        // Validate configuration at startup (Requirement 16.3: fail fast if missing)
        ValidateConfigurationCompleteness();

        // Load all markets into cache
        LoadMarketsIntoCache();

        // Register for configuration changes (Requirement 16.5: hot reload)
        _marketsConfigMonitor.OnChange(config =>
        {
            _logger.LogInformation("Market configuration changed. Reloading markets...");
            _cache.Clear();
            LoadMarketsIntoCache();
            ValidateConfigurationCompleteness();
        });

        _logger.LogInformation("Market configuration provider initialized with {MarketCount} markets", _cache.Count);
    }

    /// <summary>
    /// Retrieves the configuration for a specific market.
    /// Implements requirements 14.4, 16.1, 16.2.
    /// </summary>
    public MarketConfiguration? GetMarketConfiguration(string marketCode)
    {
        if (string.IsNullOrWhiteSpace(marketCode))
        {
            _logger.LogWarning("GetMarketConfiguration called with null or empty market code");
            return null;
        }

        if (_cache.TryGetValue(marketCode, out var config))
        {
            _logger.LogDebug("Retrieved market configuration for {MarketCode}", marketCode);
            return config;
        }

        _logger.LogWarning("Market configuration not found for market code: {MarketCode}", marketCode);
        return null;
    }

    /// <summary>
    /// Gets all configured markets.
    /// </summary>
    public IReadOnlyCollection<MarketConfiguration> GetAllMarkets()
    {
        return _cache.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Checks if a market is configured and active.
    /// Implements requirement 14.5, 14.6.
    /// </summary>
    public bool IsMarketActive(string marketCode)
    {
        var config = GetMarketConfiguration(marketCode);
        return config?.IsActive ?? false;
    }

    /// <summary>
    /// Validates that all required markets are properly configured.
    /// Implements requirement 16.3: fail fast if markets are not properly configured.
    /// </summary>
    private void ValidateConfigurationCompleteness()
    {
        var config = _marketsConfigMonitor.CurrentValue;
        
        if (config?.Markets == null || config.Markets.Count == 0)
        {
            throw new InvalidOperationException(
                "Market configuration is missing or empty. At least one market must be configured in appsettings.json under 'Markets' section.");
        }

        // Requirement 14.5: Validate that all six required markets (MA-MF) are configured
        var requiredMarkets = new[] { "MA", "MB", "MC", "MD", "ME", "MF" };
        var configuredMarkets = config.Markets.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingMarkets = requiredMarkets.Where(m => !configuredMarkets.Contains(m)).ToList();

        if (missingMarkets.Any())
        {
            throw new InvalidOperationException(
                $"Required markets are missing from configuration: {string.Join(", ", missingMarkets)}. " +
                "All six markets (MA, MB, MC, MD, ME, MF) must be configured.");
        }

        // Validate each market configuration has required properties (Requirement 16.2)
        foreach (var (marketCode, marketConfig) in config.Markets)
        {
            ValidateMarketConfig(marketCode, marketConfig);
        }

        _logger.LogInformation("Market configuration validation passed. All {RequiredMarkets} required markets are properly configured.",
            requiredMarkets.Length);
    }

    /// <summary>
    /// Validates that a single market configuration contains all required fields.
    /// Implements requirement 16.2.
    /// </summary>
    private void ValidateMarketConfig(string marketCode, MarketConfigurationOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.CountryName))
            errors.Add($"CountryName is required");

        if (string.IsNullOrWhiteSpace(options.NationalIdPattern))
            errors.Add($"NationalIdPattern is required");

        if (string.IsNullOrWhiteSpace(options.NationalIdDescription))
            errors.Add($"NationalIdDescription is required");

        if (string.IsNullOrWhiteSpace(options.NationalIdExample))
            errors.Add($"NationalIdExample is required");

        if (errors.Any())
        {
            throw new InvalidOperationException(
                $"Market configuration for '{marketCode}' is incomplete: {string.Join(", ", errors)}");
        }
    }

    /// <summary>
    /// Loads all market configurations from appsettings.json into the in-memory cache.
    /// Implements requirement 16.1 and caching as specified in task description.
    /// </summary>
    private void LoadMarketsIntoCache()
    {
        var config = _marketsConfigMonitor.CurrentValue;
        
        if (config?.Markets == null)
        {
            _logger.LogWarning("No markets configuration found");
            return;
        }

        foreach (var (marketCode, marketConfig) in config.Markets)
        {
            var nationalIdRule = new NationalIdValidationRule(
                Pattern: marketConfig.NationalIdPattern,
                Description: marketConfig.NationalIdDescription,
                Example: marketConfig.NationalIdExample
            );

            var marketConfiguration = new MarketConfiguration(
                MarketCode: marketCode,
                CountryName: marketConfig.CountryName,
                NationalIdRule: nationalIdRule,
                RequiresBranchActivation: marketConfig.RequiresBranchActivation,
                IsActive: marketConfig.IsActive
            );

            _cache[marketCode] = marketConfiguration;
            
            _logger.LogDebug("Loaded market configuration: {MarketCode} - {CountryName}, RequiresBranchActivation: {RequiresBranchActivation}",
                marketCode, marketConfig.CountryName, marketConfig.RequiresBranchActivation);
        }
    }
}

/// <summary>
/// Configuration model for binding to appsettings.json "Markets" section.
/// </summary>
public class MarketsConfiguration
{
    public Dictionary<string, MarketConfigurationOptions> Markets { get; set; } = new();
}

/// <summary>
/// Configuration options for a single market from appsettings.json.
/// Maps to the structure in appsettings.json under Markets:{MarketCode}.
/// </summary>
public class MarketConfigurationOptions
{
    public string CountryName { get; set; } = string.Empty;
    public string NationalIdPattern { get; set; } = string.Empty;
    public string NationalIdDescription { get; set; } = string.Empty;
    public string NationalIdExample { get; set; } = string.Empty;
    public bool RequiresBranchActivation { get; set; }
    public bool IsActive { get; set; } = true;
}
