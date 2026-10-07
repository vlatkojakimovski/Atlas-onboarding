namespace Atlas.Domain.Configuration;

using System.Text.RegularExpressions;

/// <summary>
/// Validates national ID formats against market-specific patterns using configuration-driven regex rules.
/// Implements requirements 2.1-2.7 for market-specific national ID validation.
/// </summary>
public class NationalIdValidator : INationalIdValidator
{
    private readonly IMarketConfigurationProvider _marketConfigurationProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="NationalIdValidator"/> class.
    /// </summary>
    /// <param name="marketConfigurationProvider">Provider for accessing market-specific configuration including national ID patterns.</param>
    public NationalIdValidator(IMarketConfigurationProvider marketConfigurationProvider)
    {
        _marketConfigurationProvider = marketConfigurationProvider ?? throw new ArgumentNullException(nameof(marketConfigurationProvider));
    }

    /// <summary>
    /// Validates a national ID against the format rules for a specific market.
    /// </summary>
    /// <param name="nationalId">The national ID to validate.</param>
    /// <param name="marketCode">The market code (MA, MB, MC, MD, ME, MF) determining validation rules.</param>
    /// <returns>True if the national ID matches the market-specific pattern; otherwise, false.</returns>
    public bool IsValid(string nationalId, string marketCode)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(marketCode))
        {
            return false;
        }

        var marketConfiguration = _marketConfigurationProvider.GetMarketConfiguration(marketCode);
        if (marketConfiguration == null)
        {
            return false;
        }

        var pattern = marketConfiguration.NationalIdRule.Pattern;
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(nationalId, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }
        catch (RegexMatchTimeoutException)
        {
            // If regex takes too long, treat as invalid for security
            return false;
        }
    }

    /// <summary>
    /// Validates a national ID and returns a validation result with error details.
    /// </summary>
    /// <param name="nationalId">The national ID to validate.</param>
    /// <param name="marketCode">The market code determining validation rules.</param>
    /// <returns>A validation result containing success status and error message if validation fails.</returns>
    public NationalIdValidationResult ValidateWithDetails(string nationalId, string marketCode)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
        {
            return new NationalIdValidationResult(
                IsValid: false,
                ErrorMessage: "National ID is required and cannot be empty.",
                ExpectedFormat: null);
        }

        if (string.IsNullOrWhiteSpace(marketCode))
        {
            return new NationalIdValidationResult(
                IsValid: false,
                ErrorMessage: "Market code is required.",
                ExpectedFormat: null);
        }

        var marketConfiguration = _marketConfigurationProvider.GetMarketConfiguration(marketCode);
        if (marketConfiguration == null)
        {
            return new NationalIdValidationResult(
                IsValid: false,
                ErrorMessage: $"Market code '{marketCode}' is not supported.",
                ExpectedFormat: null);
        }

        var pattern = marketConfiguration.NationalIdRule.Pattern;
        var description = marketConfiguration.NationalIdRule.Description;
        var example = marketConfiguration.NationalIdRule.Example;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return new NationalIdValidationResult(
                IsValid: false,
                ErrorMessage: $"National ID validation pattern is not configured for market {marketCode}.",
                ExpectedFormat: description);
        }

        try
        {
            bool isMatch = Regex.IsMatch(nationalId, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

            if (isMatch)
            {
                return new NationalIdValidationResult(
                    IsValid: true,
                    ErrorMessage: null,
                    ExpectedFormat: description);
            }
            else
            {
                string errorMessage = $"National ID format is invalid for market {marketCode}. Expected format: {description}";
                if (!string.IsNullOrWhiteSpace(example))
                {
                    errorMessage += $" (example: {example})";
                }

                return new NationalIdValidationResult(
                    IsValid: false,
                    ErrorMessage: errorMessage,
                    ExpectedFormat: description);
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return new NationalIdValidationResult(
                IsValid: false,
                ErrorMessage: "National ID validation timed out. Please check the format.",
                ExpectedFormat: description);
        }
    }
}
