namespace Atlas.Application.Validation;

using Atlas.Application.Commands;

/// <summary>
/// Defines the contract for validating application submission commands.
/// Implements requirements 1.2-1.8, 2.7.
/// </summary>
public interface IApplicationValidator
{
    /// <summary>
    /// Validates a submit application command against all business rules.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>A validation result containing success status and any errors.</returns>
    ValidationResult Validate(SubmitApplicationCommand command);
}
