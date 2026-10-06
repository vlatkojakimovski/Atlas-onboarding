namespace Atlas.Domain.ValueObjects;

/// <summary>
/// Represents the validation rule for national ID format in a specific market.
/// </summary>
/// <param name="Pattern">Regular expression pattern for validating the national ID format.</param>
/// <param name="Description">Human-readable description of the expected format.</param>
/// <param name="Example">Example of a valid national ID in this format.</param>
public record NationalIdValidationRule(
    string Pattern,
    string Description,
    string Example);
