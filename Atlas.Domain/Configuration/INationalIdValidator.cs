namespace Atlas.Domain.Configuration;

/// <summary>
/// Validates national ID formats against market-specific patterns.
/// </summary>
public interface INationalIdValidator
{
    /// <summary>
    /// Validates a national ID against the format rules for a specific market.
    /// </summary>
    /// <param name="nationalId">The national ID to validate.</param>
    /// <param name="marketCode">The market code (MA, MB, MC, MD, ME, MF) determining validation rules.</param>
    /// <returns>True if the national ID matches the market-specific pattern; otherwise, false.</returns>
    bool IsValid(string nationalId, string marketCode);

    /// <summary>
    /// Validates a national ID and returns a validation result with error details.
    /// </summary>
    /// <param name="nationalId">The national ID to validate.</param>
    /// <param name="marketCode">The market code determining validation rules.</param>
    /// <returns>A validation result containing success status and error message if validation fails.</returns>
    NationalIdValidationResult ValidateWithDetails(string nationalId, string marketCode);
}

/// <summary>
/// Result of national ID validation containing success status and error information.
/// </summary>
/// <param name="IsValid">True if the national ID is valid for the market; otherwise, false.</param>
/// <param name="ErrorMessage">Detailed error message if validation failed; otherwise, null.</param>
/// <param name="ExpectedFormat">Description of the expected format for the market.</param>
public record NationalIdValidationResult(
    bool IsValid,
    string? ErrorMessage,
    string? ExpectedFormat);
