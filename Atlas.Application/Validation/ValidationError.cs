namespace Atlas.Application.Validation;

/// <summary>
/// Represents a single validation error with field information and error details.
/// </summary>
/// <param name="Field">The name of the field that failed validation.</param>
/// <param name="Code">A machine-readable error code for the validation failure.</param>
/// <param name="Message">A human-readable error message describing the validation failure.</param>
public record ValidationError(
    string Field,
    string Code,
    string Message);
