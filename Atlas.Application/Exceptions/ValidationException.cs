namespace Atlas.Application.Exceptions;

using Atlas.Application.Validation;

/// <summary>
/// Exception thrown when application validation fails.
/// Contains the validation errors for client feedback.
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(IReadOnlyList<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors ?? Array.Empty<ValidationError>();
    }

    public ValidationException(ValidationError error)
        : this(new[] { error })
    {
    }

    public ValidationException(string field, string code, string message)
        : this(new ValidationError(field, code, message))
    {
    }
}
