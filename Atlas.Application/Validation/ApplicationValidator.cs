namespace Atlas.Application.Validation;

using Atlas.Application.Commands;
using Atlas.Domain.Configuration;
using System.Text.RegularExpressions;

/// <summary>
/// Validates application submission commands against all business rules.
/// Implements requirements 1.2-1.8, 2.7.
/// </summary>
public class ApplicationValidator : IApplicationValidator
{
    private readonly IMarketConfigurationProvider _marketConfigurationProvider;
    private readonly INationalIdValidator _nationalIdValidator;

    // Simple email regex pattern
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100));

    // E.164 phone pattern (optional '+' followed by 1-15 digits)
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    // Base64 pattern for basic validation
    private static readonly Regex Base64Regex = new(
        @"^[A-Za-z0-9+/]*={0,2}$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    public ApplicationValidator(
        IMarketConfigurationProvider marketConfigurationProvider,
        INationalIdValidator nationalIdValidator)
    {
        _marketConfigurationProvider = marketConfigurationProvider ?? throw new ArgumentNullException(nameof(marketConfigurationProvider));
        _nationalIdValidator = nationalIdValidator ?? throw new ArgumentNullException(nameof(nationalIdValidator));
    }

    /// <summary>
    /// Validates the application command against all business rules.
    /// </summary>
    public ValidationResult Validate(SubmitApplicationCommand command)
    {
        if (command == null)
        {
            return ValidationResult.Failure(new ValidationError(
                "Command",
                "NULL_COMMAND",
                "Command cannot be null"));
        }

        var errors = new List<ValidationError>();

        // Validate required fields
        ValidateRequiredFields(command, errors);

        // Validate field formats
        ValidateFieldFormats(command, errors);

        // Validate market code
        ValidateMarketCode(command.Market, errors);

        // Validate national ID format (only if market is valid)
        if (!string.IsNullOrWhiteSpace(command.Market))
        {
            ValidateNationalId(command.NationalId, command.Market, errors);
        }

        // Validate age (18+)
        ValidateAge(command.DateOfBirth, errors);

        // Validate documents
        ValidateDocuments(command.Documents, errors);

        // Validate terms accepted
        ValidateTermsAccepted(command.TermsAccepted, errors);

        return errors.Any()
            ? ValidationResult.Failure(errors)
            : ValidationResult.Success();
    }

    /// <summary>
    /// Validates that all required fields are provided.
    /// Requirement 1.2: All customer data fields are required.
    /// </summary>
    private void ValidateRequiredFields(SubmitApplicationCommand command, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            errors.Add(new ValidationError(
                nameof(command.FirstName),
                "REQUIRED",
                "First name is required"));
        }

        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            errors.Add(new ValidationError(
                nameof(command.LastName),
                "REQUIRED",
                "Last name is required"));
        }

        if (command.DateOfBirth == default)
        {
            errors.Add(new ValidationError(
                nameof(command.DateOfBirth),
                "REQUIRED",
                "Date of birth is required"));
        }

        if (string.IsNullOrWhiteSpace(command.Market))
        {
            errors.Add(new ValidationError(
                nameof(command.Market),
                "REQUIRED",
                "Market code is required"));
        }

        if (string.IsNullOrWhiteSpace(command.NationalId))
        {
            errors.Add(new ValidationError(
                nameof(command.NationalId),
                "REQUIRED",
                "National ID is required"));
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            errors.Add(new ValidationError(
                nameof(command.Email),
                "REQUIRED",
                "Email is required"));
        }

        if (string.IsNullOrWhiteSpace(command.Phone))
        {
            errors.Add(new ValidationError(
                nameof(command.Phone),
                "REQUIRED",
                "Phone number is required"));
        }

        if (command.Documents == null || command.Documents.Count == 0)
        {
            errors.Add(new ValidationError(
                nameof(command.Documents),
                "REQUIRED",
                "At least one document is required"));
        }
    }

    /// <summary>
    /// Validates field formats and lengths.
    /// Requirement 1.3: Field length constraints.
    /// </summary>
    private void ValidateFieldFormats(SubmitApplicationCommand command, List<ValidationError> errors)
    {
        // First name: 1-100 characters
        if (!string.IsNullOrWhiteSpace(command.FirstName) &&
            (command.FirstName.Length < 1 || command.FirstName.Length > 100))
        {
            errors.Add(new ValidationError(
                nameof(command.FirstName),
                "INVALID_LENGTH",
                "First name must be between 1 and 100 characters"));
        }

        // Last name: 1-100 characters
        if (!string.IsNullOrWhiteSpace(command.LastName) &&
            (command.LastName.Length < 1 || command.LastName.Length > 100))
        {
            errors.Add(new ValidationError(
                nameof(command.LastName),
                "INVALID_LENGTH",
                "Last name must be between 1 and 100 characters"));
        }

        // Email format
        if (!string.IsNullOrWhiteSpace(command.Email) &&
            !EmailRegex.IsMatch(command.Email))
        {
            errors.Add(new ValidationError(
                nameof(command.Email),
                "INVALID_FORMAT",
                "Email format is invalid"));
        }

        // Phone format (E.164 recommended but not strictly enforced)
        if (!string.IsNullOrWhiteSpace(command.Phone) &&
            !PhoneRegex.IsMatch(command.Phone))
        {
            errors.Add(new ValidationError(
                nameof(command.Phone),
                "INVALID_FORMAT",
                "Phone number format is invalid. E.164 format recommended (e.g., +1234567890)"));
        }
    }

    /// <summary>
    /// Validates market code against supported markets.
    /// Requirement 2.1: Market code must be one of MA, MB, MC, MD, ME, MF.
    /// </summary>
    private void ValidateMarketCode(string market, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(market))
            return; // Already handled in required fields

        var marketConfig = _marketConfigurationProvider.GetMarketConfiguration(market);
        if (marketConfig == null)
        {
            errors.Add(new ValidationError(
                "Market",
                "INVALID_MARKET",
                $"Market code '{market}' is not supported. Supported markets: MA, MB, MC, MD, ME, MF"));
        }
        else if (!marketConfig.IsActive)
        {
            errors.Add(new ValidationError(
                "Market",
                "MARKET_INACTIVE",
                $"Market '{market}' is currently inactive for onboarding"));
        }
    }

    /// <summary>
    /// Validates national ID format against market-specific rules.
    /// Requirement 2.7: National ID must conform to market-specific format.
    /// </summary>
    private void ValidateNationalId(string nationalId, string market, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(nationalId) || string.IsNullOrWhiteSpace(market))
            return; // Already handled elsewhere

        var validationResult = _nationalIdValidator.ValidateWithDetails(nationalId, market);
        if (!validationResult.IsValid)
        {
            errors.Add(new ValidationError(
                nameof(SubmitApplicationCommand.NationalId),
                "INVALID_FORMAT",
                validationResult.ErrorMessage ?? "National ID format is invalid"));
        }
    }

    /// <summary>
    /// Validates applicant age (must be 18+).
    /// Requirement 1.8: Applicant must be at least 18 years old.
    /// </summary>
    private void ValidateAge(DateTime dateOfBirth, List<ValidationError> errors)
    {
        if (dateOfBirth == default)
            return; // Already handled in required fields

        var age = DateTime.UtcNow.Year - dateOfBirth.Year;
        if (dateOfBirth > DateTime.UtcNow.AddYears(-age))
            age--; // Account for birthday not yet occurred this year

        if (age < 18)
        {
            errors.Add(new ValidationError(
                nameof(SubmitApplicationCommand.DateOfBirth),
                "UNDERAGE",
                "Applicant must be at least 18 years old"));
        }
    }

    /// <summary>
    /// Validates document submissions.
    /// Requirement 1.4: At least one ID document (PASSPORT or ID_CARD) required.
    /// Requirement 1.5: One SELFIE document required.
    /// </summary>
    private void ValidateDocuments(IReadOnlyList<DocumentSubmission> documents, List<ValidationError> errors)
    {
        if (documents == null || documents.Count == 0)
            return; // Already handled in required fields

        var documentTypes = documents
            .Where(d => !string.IsNullOrWhiteSpace(d.Type))
            .Select(d => d.Type.ToUpperInvariant())
            .ToList();

        // Check for at least one ID document (PASSPORT or ID_CARD)
        var hasIdDocument = documentTypes.Contains("PASSPORT") || documentTypes.Contains("ID_CARD");
        if (!hasIdDocument)
        {
            errors.Add(new ValidationError(
                nameof(SubmitApplicationCommand.Documents),
                "MISSING_ID_DOCUMENT",
                "At least one identity document (PASSPORT or ID_CARD) is required"));
        }

        // Check for SELFIE
        var hasSelfie = documentTypes.Contains("SELFIE");
        if (!hasSelfie)
        {
            errors.Add(new ValidationError(
                nameof(SubmitApplicationCommand.Documents),
                "MISSING_SELFIE",
                "A SELFIE document is required for face matching"));
        }

        // Validate each document's image data
        for (int i = 0; i < documents.Count; i++)
        {
            var doc = documents[i];

            if (string.IsNullOrWhiteSpace(doc.Type))
            {
                errors.Add(new ValidationError(
                    $"{nameof(SubmitApplicationCommand.Documents)}[{i}].Type",
                    "REQUIRED",
                    $"Document type is required for document at index {i}"));
            }
            else if (!new[] { "PASSPORT", "ID_CARD", "SELFIE" }.Contains(doc.Type.ToUpperInvariant()))
            {
                errors.Add(new ValidationError(
                    $"{nameof(SubmitApplicationCommand.Documents)}[{i}].Type",
                    "INVALID_TYPE",
                    $"Invalid document type '{doc.Type}'. Must be PASSPORT, ID_CARD, or SELFIE"));
            }

            if (string.IsNullOrWhiteSpace(doc.Image))
            {
                errors.Add(new ValidationError(
                    $"{nameof(SubmitApplicationCommand.Documents)}[{i}].Image",
                    "REQUIRED",
                    $"Image data is required for document at index {i}"));
            }
            else
            {
                // Basic base64 validation
                ValidateBase64Image(doc.Image, i, errors);
            }
        }
    }

    /// <summary>
    /// Validates base64 image data integrity.
    /// Requirement 1.6: Image data must be valid base64.
    /// </summary>
    private void ValidateBase64Image(string imageData, int documentIndex, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(imageData))
            return;

        // Check if it's valid base64
        if (!Base64Regex.IsMatch(imageData) || imageData.Length % 4 != 0)
        {
            errors.Add(new ValidationError(
                $"{nameof(SubmitApplicationCommand.Documents)}[{documentIndex}].Image",
                "INVALID_BASE64",
                $"Image data for document at index {documentIndex} is not valid base64"));
            return;
        }

        // Try to decode to verify
        try
        {
            var bytes = Convert.FromBase64String(imageData);
            if (bytes.Length == 0)
            {
                errors.Add(new ValidationError(
                    $"{nameof(SubmitApplicationCommand.Documents)}[{documentIndex}].Image",
                    "EMPTY_IMAGE",
                    $"Image data for document at index {documentIndex} is empty"));
            }
        }
        catch (FormatException)
        {
            errors.Add(new ValidationError(
                $"{nameof(SubmitApplicationCommand.Documents)}[{documentIndex}].Image",
                "INVALID_BASE64",
                $"Image data for document at index {documentIndex} could not be decoded"));
        }
    }

    /// <summary>
    /// Validates terms acceptance.
    /// Requirement 1.7: Terms and conditions must be accepted.
    /// </summary>
    private void ValidateTermsAccepted(bool termsAccepted, List<ValidationError> errors)
    {
        if (!termsAccepted)
        {
            errors.Add(new ValidationError(
                nameof(SubmitApplicationCommand.TermsAccepted),
                "TERMS_NOT_ACCEPTED",
                "Terms and conditions must be accepted to proceed"));
        }
    }
}
