namespace Atlas.Domain.Validation;

/// <summary>
/// Validates application data before creating an Application entity.
/// </summary>
public static class ApplicationDataValidator
{
    /// <summary>
    /// Validates all application data and returns a ValidationResult.
    /// </summary>
    public static ValidationResult ValidateApplicationData(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string marketCode,
        string nationalId,
        string email,
        string phone)
    {
        var result = new ValidationResult();

        // Validate first name
        if (!DomainValidationRules.IsValidName(firstName))
            result.AddError($"First name: {DomainValidationRules.NameValidationMessage}");

        // Validate last name
        if (!DomainValidationRules.IsValidName(lastName))
            result.AddError($"Last name: {DomainValidationRules.NameValidationMessage}");

        // Validate age
        if (!DomainValidationRules.IsValidAge(dateOfBirth))
            result.AddError(DomainValidationRules.AgeValidationMessage);

        // Validate market code
        if (!DomainValidationRules.IsValidMarketCode(marketCode))
            result.AddError(DomainValidationRules.MarketCodeValidationMessage);

        // Validate national ID
        if (!DomainValidationRules.IsValidNationalId(nationalId))
            result.AddError(DomainValidationRules.NationalIdValidationMessage);

        // Validate email
        if (!DomainValidationRules.IsValidEmail(email))
            result.AddError(DomainValidationRules.EmailValidationMessage);

        // Validate phone
        if (!DomainValidationRules.IsValidPhone(phone))
            result.AddError(DomainValidationRules.PhoneValidationMessage);

        return result;
    }
}
