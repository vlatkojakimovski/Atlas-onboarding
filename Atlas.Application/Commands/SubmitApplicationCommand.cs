namespace Atlas.Application.Commands;

/// <summary>
/// Command to submit a new customer onboarding application.
/// Contains all required customer information and documents for processing.
/// </summary>
/// <param name="FirstName">Customer's first name (1-100 characters).</param>
/// <param name="LastName">Customer's last name (1-100 characters).</param>
/// <param name="DateOfBirth">Customer's date of birth (must be 18+ years old).</param>
/// <param name="Market">Market code (MA, MB, MC, MD, ME, MF).</param>
/// <param name="NationalId">National ID conforming to market-specific format.</param>
/// <param name="Email">Customer's email address.</param>
/// <param name="Phone">Customer's phone number (E.164 format recommended).</param>
/// <param name="Documents">Collection of identity documents (ID/Passport + Selfie required).</param>
/// <param name="TermsAccepted">Whether customer accepted terms and conditions (must be true).</param>
public record SubmitApplicationCommand(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Market,
    string NationalId,
    string Email,
    string Phone,
    IReadOnlyList<DocumentSubmission> Documents,
    bool TermsAccepted);

/// <summary>
/// Represents a document submission with type and image data.
/// </summary>
/// <param name="Type">Document type (PASSPORT, ID_CARD, or SELFIE).</param>
/// <param name="Image">Base64-encoded image data.</param>
public record DocumentSubmission(
    string Type,
    string Image);
