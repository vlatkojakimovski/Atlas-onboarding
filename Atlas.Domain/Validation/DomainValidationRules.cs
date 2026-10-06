namespace Atlas.Domain.Validation;

/// <summary>
/// Encapsulates domain validation rules for customer applications.
/// </summary>
public static class DomainValidationRules
{
    /// <summary>
    /// Minimum age requirement for applicants.
    /// </summary>
    public const int MinimumAge = 18;

    /// <summary>
    /// Minimum length for first and last names.
    /// </summary>
    public const int MinNameLength = 1;

    /// <summary>
    /// Maximum length for first and last names.
    /// </summary>
    public const int MaxNameLength = 100;

    /// <summary>
    /// Maximum length for email addresses.
    /// </summary>
    public const int MaxEmailLength = 255;

    /// <summary>
    /// Maximum length for phone numbers.
    /// </summary>
    public const int MaxPhoneLength = 20;

    /// <summary>
    /// Maximum length for national ID.
    /// </summary>
    public const int MaxNationalIdLength = 50;

    /// <summary>
    /// Validates that the applicant is at least 18 years old.
    /// </summary>
    /// <param name="dateOfBirth">The applicant's date of birth.</param>
    /// <returns>True if the applicant is 18 or older; otherwise, false.</returns>
    public static bool IsValidAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;
        
        // Adjust if birthday hasn't occurred this year yet
        if (dateOfBirth > today.AddYears(-age))
            age--;

        return age >= MinimumAge;
    }

    /// <summary>
    /// Validates that a name (first or last) meets length requirements.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <returns>True if the name is valid; otherwise, false.</returns>
    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) 
               && name.Length >= MinNameLength 
               && name.Length <= MaxNameLength;
    }

    /// <summary>
    /// Validates that an email meets length requirements.
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <returns>True if the email is valid; otherwise, false.</returns>
    public static bool IsValidEmail(string? email)
    {
        return !string.IsNullOrWhiteSpace(email) && email.Length <= MaxEmailLength;
    }

    /// <summary>
    /// Validates that a phone number meets length requirements.
    /// </summary>
    /// <param name="phone">The phone number to validate.</param>
    /// <returns>True if the phone is valid; otherwise, false.</returns>
    public static bool IsValidPhone(string? phone)
    {
        return !string.IsNullOrWhiteSpace(phone) && phone.Length <= MaxPhoneLength;
    }

    /// <summary>
    /// Validates that a national ID meets length requirements.
    /// </summary>
    /// <param name="nationalId">The national ID to validate.</param>
    /// <returns>True if the national ID is valid; otherwise, false.</returns>
    public static bool IsValidNationalId(string? nationalId)
    {
        return !string.IsNullOrWhiteSpace(nationalId) && nationalId.Length <= MaxNationalIdLength;
    }

    /// <summary>
    /// Validates that a market code is provided.
    /// </summary>
    /// <param name="marketCode">The market code to validate.</param>
    /// <returns>True if the market code is valid; otherwise, false.</returns>
    public static bool IsValidMarketCode(string? marketCode)
    {
        return !string.IsNullOrWhiteSpace(marketCode);
    }

    /// <summary>
    /// Gets the validation error message for age requirement.
    /// </summary>
    public static string AgeValidationMessage => $"Applicant must be at least {MinimumAge} years old.";

    /// <summary>
    /// Gets the validation error message for name requirement.
    /// </summary>
    public static string NameValidationMessage => $"Name must be between {MinNameLength} and {MaxNameLength} characters.";

    /// <summary>
    /// Gets the validation error message for email requirement.
    /// </summary>
    public static string EmailValidationMessage => $"Email is required and must not exceed {MaxEmailLength} characters.";

    /// <summary>
    /// Gets the validation error message for phone requirement.
    /// </summary>
    public static string PhoneValidationMessage => $"Phone is required and must not exceed {MaxPhoneLength} characters.";

    /// <summary>
    /// Gets the validation error message for national ID requirement.
    /// </summary>
    public static string NationalIdValidationMessage => $"National ID is required and must not exceed {MaxNationalIdLength} characters.";

    /// <summary>
    /// Gets the validation error message for market code requirement.
    /// </summary>
    public static string MarketCodeValidationMessage => "Market code is required.";
}
