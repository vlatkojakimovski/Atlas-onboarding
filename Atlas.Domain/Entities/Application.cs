using Atlas.Domain.Enums;
using Atlas.Domain.ValueObjects;

namespace Atlas.Domain.Entities;

/// <summary>
/// Represents a customer onboarding application aggregate root.
/// Encapsulates all business logic and state transitions for the application lifecycle.
/// </summary>
public class Application
{
    // Private constructor for EF Core
    private Application()
    {
        Documents = new List<Document>();
    }

    /// <summary>
    /// Creates a new application with the provided customer details.
    /// </summary>
    public Application(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string marketCode,
        string nationalId,
        string email,
        string phone)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > 100)
            throw new ArgumentException("First name is required and must be 1-100 characters.", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length > 100)
            throw new ArgumentException("Last name is required and must be 1-100 characters.", nameof(lastName));
        
        if (dateOfBirth >= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
            throw new ArgumentException("Applicant must be at least 18 years old.", nameof(dateOfBirth));
        
        if (string.IsNullOrWhiteSpace(marketCode))
            throw new ArgumentException("Market code is required.", nameof(marketCode));
        
        if (string.IsNullOrWhiteSpace(nationalId))
            throw new ArgumentException("National ID is required.", nameof(nationalId));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        // Initialize
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        MarketCode = marketCode;
        NationalId = nationalId;
        Email = email;
        Phone = phone;
        Status = ApplicationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Documents = new List<Document>();
    }

    // Properties with private setters for encapsulation
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string MarketCode { get; private set; } = string.Empty;
    public string NationalId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public ApplicationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Navigation properties
    public ICollection<Document> Documents { get; private set; }
    public IdentityVerification? IdentityVerification { get; private set; }
    public SanctionsScreening? SanctionsScreening { get; private set; }
    public AccountDetails? AccountDetails { get; private set; }
    public CardOrder? CardOrder { get; private set; }
    public ApplicationDecision? Decision { get; private set; }

    /// <summary>
    /// Starts the verification process for this application.
    /// </summary>
    public void StartVerification()
    {
        EnsureStatus(ApplicationStatus.Pending, "Cannot start verification");
        // Verification is started externally; this method can be used for state tracking if needed
    }

    /// <summary>
    /// Records the completion of identity verification.
    /// </summary>
    public void CompleteVerification(IdentityVerificationResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        EnsureStatus(ApplicationStatus.Pending, "Cannot complete verification");

        IdentityVerification = new IdentityVerification(
            Id,
            result.ProviderId,
            result.DocumentStatus,
            result.FaceMatch,
            result.Confidence,
            result.VerifiedAt);
    }

    /// <summary>
    /// Records the completion of sanctions screening.
    /// </summary>
    public void CompleteScreening(SanctionsScreeningResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        EnsureStatus(ApplicationStatus.Pending, "Cannot complete screening");

        var matches = result.Matches
            .Select(m => new SanctionMatch(m.Type, m.Score, m.Subject))
            .ToList();

        SanctionsScreening = new SanctionsScreening(
            Id,
            result.CaseId,
            result.Status,
            result.ScreenedAt,
            matches);
    }

    /// <summary>
    /// Approves the application and records account and card details.
    /// </summary>
    public void Approve(Guid accountId, string accountNumber, Guid cardOrderId, string cardReference, bool requiresBranchActivation)
    {
        EnsureStatus(ApplicationStatus.Pending, "Cannot approve application");

        if (IdentityVerification == null)
            throw new InvalidOperationException("Cannot approve application without identity verification.");

        if (SanctionsScreening == null)
            throw new InvalidOperationException("Cannot approve application without sanctions screening.");

        // Create account details
        AccountDetails = new AccountDetails(Id, accountId, accountNumber, DateTime.UtcNow);

        // Create card order
        CardOrder = new CardOrder(Id, accountId, cardOrderId, cardReference, requiresBranchActivation, DateTime.UtcNow);

        // Create decision
        Decision = new ApplicationDecision(Id, ApplicationStatus.Approved, DateTime.UtcNow, new List<RejectionReason>());

        // Update status
        Status = ApplicationStatus.Approved;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rejects the application with the specified reasons.
    /// </summary>
    public void Reject(IReadOnlyList<RejectionReason> reasons)
    {
        if (reasons == null || reasons.Count == 0)
            throw new ArgumentException("At least one rejection reason is required.", nameof(reasons));

        EnsureStatus(ApplicationStatus.Pending, "Cannot reject application");

        // Create decision with rejection reasons
        Decision = new ApplicationDecision(Id, ApplicationStatus.Rejected, DateTime.UtcNow, reasons.ToList());

        // Update status
        Status = ApplicationStatus.Rejected;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a document to the application.
    /// </summary>
    public void AddDocument(DocumentType type, byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
            throw new ArgumentException("Image data is required.", nameof(imageData));

        EnsureStatus(ApplicationStatus.Pending, "Cannot add documents");

        var document = new Document(Id, type, imageData, DateTime.UtcNow);
        Documents.Add(document);
    }

    private void EnsureStatus(ApplicationStatus expectedStatus, string message)
    {
        if (Status != expectedStatus)
            throw new InvalidOperationException($"{message}. Current status: {Status}, expected: {expectedStatus}.");
    }
}
