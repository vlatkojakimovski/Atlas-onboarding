namespace Atlas.Application.Validation;

/// <summary>
/// Represents the result of validation with success status and error collection.
/// </summary>
/// <param name="IsValid">True if validation passed; otherwise, false.</param>
/// <param name="Errors">Collection of validation errors (empty if valid).</param>
public record ValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationError> Errors)
{
    /// <summary>
    /// Creates a successful validation result with no errors.
    /// </summary>
    public static ValidationResult Success() => new(true, Array.Empty<ValidationError>());

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    public static ValidationResult Failure(IEnumerable<ValidationError> errors) => 
        new(false, errors.ToList().AsReadOnly());

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    public static ValidationResult Failure(ValidationError error) => 
        new(false, new[] { error });
}
