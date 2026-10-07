namespace Atlas.Api.Contracts;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Request model for submitting a new customer onboarding application.
/// Maps to the API contract with camelCase JSON property names.
/// </summary>
public record SubmitApplicationRequest
{
    [JsonPropertyName("firstName")]
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
    public string FirstName { get; init; } = string.Empty;

    [JsonPropertyName("lastName")]
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
    public string LastName { get; init; } = string.Empty;

    [JsonPropertyName("dateOfBirth")]
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; init; }

    [JsonPropertyName("market")]
    [Required(ErrorMessage = "Market code is required")]
    [RegularExpression("^(MA|MB|MC|MD|ME|MF)$", ErrorMessage = "Market must be one of: MA, MB, MC, MD, ME, MF")]
    public string Market { get; init; } = string.Empty;

    [JsonPropertyName("nationalId")]
    [Required(ErrorMessage = "National ID is required")]
    public string NationalId { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("phone")]
    [Required(ErrorMessage = "Phone number is required")]
    public string Phone { get; init; } = string.Empty;

    [JsonPropertyName("documents")]
    [Required(ErrorMessage = "Documents are required")]
    [MinLength(1, ErrorMessage = "At least one document is required")]
    public List<DocumentSubmissionDto> Documents { get; init; } = new();

    [JsonPropertyName("termsAccepted")]
    [Required(ErrorMessage = "Terms acceptance is required")]
    public bool TermsAccepted { get; init; }
}

/// <summary>
/// Represents a document submission in the API request.
/// </summary>
public record DocumentSubmissionDto
{
    [JsonPropertyName("type")]
    [Required(ErrorMessage = "Document type is required")]
    [RegularExpression("^(PASSPORT|ID_CARD|SELFIE)$", ErrorMessage = "Document type must be PASSPORT, ID_CARD, or SELFIE")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("image")]
    [Required(ErrorMessage = "Document image is required")]
    public string Image { get; init; } = string.Empty;
}
