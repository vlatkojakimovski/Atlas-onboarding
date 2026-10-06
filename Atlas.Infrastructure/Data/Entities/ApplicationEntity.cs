namespace Atlas.Infrastructure.Data.Entities;

public class ApplicationEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string MarketCode { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected, PendingReview
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CreatedBy { get; set; }

    // Navigation properties
    public ICollection<DocumentEntity> Documents { get; set; } = new List<DocumentEntity>();
    public IdentityVerificationEntity? IdentityVerification { get; set; }
    public SanctionsScreeningEntity? SanctionsScreening { get; set; }
    public AccountDetailsEntity? AccountDetails { get; set; }
    public CardOrderEntity? CardOrder { get; set; }
    public ApplicationDecisionEntity? Decision { get; set; }
    public ICollection<AuditLogEntity> AuditLogs { get; set; } = new List<AuditLogEntity>();
}
