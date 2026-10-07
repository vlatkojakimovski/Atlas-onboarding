namespace Atlas.Infrastructure.Audit;

using Atlas.Domain.Enums;
using Atlas.Domain.ValueObjects;
using Atlas.Infrastructure.Data;
using Atlas.Infrastructure.Data.Entities;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation of audit logger that emits structured logs and persists audit events to database.
/// Implements requirements 10.1-10.10, 18.1-18.8.
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly ApplicationDbContext _dbContext;

    public AuditLogger(
        ILogger<AuditLogger> logger,
        ApplicationDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Logs application creation event.
    /// </summary>
    public async Task LogApplicationCreatedAsync(
        Guid applicationId,
        string marketCode,
        string userId,
        CancellationToken cancellationToken = default)
    {
        const string eventType = "ApplicationCreated";

        // Emit structured log (Requirement 18.1-18.8)
        _logger.LogInformation(
            "AUDIT: {EventType} - ApplicationId: {ApplicationId}, MarketCode: {MarketCode}, UserId: {UserId}",
            eventType, applicationId, marketCode, userId);

        // Persist to database for 10-year retention (Requirement 10.9)
        var auditLog = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            EventType = eventType,
            UserId = userId,
            MarketCode = marketCode,
            Details = $"Application created in market {marketCode}",
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Logs identity verification completion event.
    /// </summary>
    public async Task LogVerificationCompletedAsync(
        Guid applicationId,
        IdentityVerificationResult verificationResult,
        CancellationToken cancellationToken = default)
    {
        const string eventType = "VerificationCompleted";

        // Emit structured log
        _logger.LogInformation(
            "AUDIT: {EventType} - ApplicationId: {ApplicationId}, ProviderId: {ProviderId}, DocumentStatus: {DocumentStatus}, FaceMatch: {FaceMatch}, Confidence: {Confidence}",
            eventType, applicationId, verificationResult.ProviderId, verificationResult.DocumentStatus, verificationResult.FaceMatch, verificationResult.Confidence);

        // Persist to database
        var auditLog = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            EventType = eventType,
            Details = $"Verification completed - ProviderId: {verificationResult.ProviderId}, Status: {verificationResult.DocumentStatus}, FaceMatch: {verificationResult.FaceMatch}, Confidence: {verificationResult.Confidence}",
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Logs sanctions screening completion event.
    /// </summary>
    public async Task LogScreeningCompletedAsync(
        Guid applicationId,
        SanctionsScreeningResult screeningResult,
        CancellationToken cancellationToken = default)
    {
        const string eventType = "ScreeningCompleted";

        // Emit structured log
        _logger.LogInformation(
            "AUDIT: {EventType} - ApplicationId: {ApplicationId}, CaseId: {CaseId}, Status: {Status}, MatchCount: {MatchCount}",
            eventType, applicationId, screeningResult.CaseId, screeningResult.Status, screeningResult.Matches.Count);

        // Persist to database
        var matchDetails = screeningResult.Matches.Any()
            ? string.Join(", ", screeningResult.Matches.Select(m => $"{m.Type}: {m.Subject} (Score: {m.Score})"))
            : "No matches";

        var auditLog = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            EventType = eventType,
            Details = $"Screening completed - CaseId: {screeningResult.CaseId}, Status: {screeningResult.Status}, Matches: [{matchDetails}]",
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Logs decision made on application.
    /// </summary>
    public async Task LogDecisionMadeAsync(
        Guid applicationId,
        ApplicationDecision decision,
        CancellationToken cancellationToken = default)
    {
        const string eventType = "DecisionMade";

        // Emit structured log
        _logger.LogInformation(
            "AUDIT: {EventType} - ApplicationId: {ApplicationId}, Status: {Status}, IsApproved: {IsApproved}, IsRejected: {IsRejected}, ReasonCount: {ReasonCount}",
            eventType, applicationId, decision.Status, decision.IsApproved, decision.IsRejected, decision.Reasons.Count);

        // Persist to database
        var reasonsText = decision.Reasons.Any()
            ? string.Join(", ", decision.Reasons)
            : "None";

        var auditLog = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            EventType = eventType,
            Details = $"Decision: {decision.Status}, Reasons: [{reasonsText}]",
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Logs account opening event.
    /// </summary>
    public async Task LogAccountOpenedAsync(
        Guid applicationId,
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        const string eventType = "AccountOpened";

        // Emit structured log
        _logger.LogInformation(
            "AUDIT: {EventType} - ApplicationId: {ApplicationId}, AccountId: {AccountId}",
            eventType, applicationId, accountId);

        // Persist to database
        var auditLog = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            EventType = eventType,
            Details = $"Account opened - AccountId: {accountId}",
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Logs card ordering event.
    /// </summary>
    public async Task LogCardOrderedAsync(
        Guid applicationId,
        Guid cardOrderId,
        CancellationToken cancellationToken = default)
    {
        const string eventType = "CardOrdered";

        // Emit structured log
        _logger.LogInformation(
            "AUDIT: {EventType} - ApplicationId: {ApplicationId}, CardOrderId: {CardOrderId}",
            eventType, applicationId, cardOrderId);

        // Persist to database
        var auditLog = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            EventType = eventType,
            Details = $"Card ordered - CardOrderId: {cardOrderId}",
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Logs application status change event.
    /// </summary>
    public async Task LogApplicationStatusChangedAsync(
        Guid applicationId,
        ApplicationStatus fromStatus,
        ApplicationStatus toStatus,
        string reason,
        CancellationToken cancellationToken = default)
    {
        const string eventType = "ApplicationStatusChanged";

        // Emit structured log
        _logger.LogInformation(
            "AUDIT: {EventType} - ApplicationId: {ApplicationId}, FromStatus: {FromStatus}, ToStatus: {ToStatus}, Reason: {Reason}",
            eventType, applicationId, fromStatus, toStatus, reason);

        // Persist to database
        var auditLog = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            EventType = eventType,
            Details = $"Status changed from {fromStatus} to {toStatus}. Reason: {reason}",
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Logs data access event for compliance.
    /// </summary>
    public async Task LogDataAccessAsync(
        string userId,
        string resourceType,
        string resourceId,
        string action,
        CancellationToken cancellationToken = default)
    {
        const string eventType = "DataAccess";

        // Emit structured log
        _logger.LogInformation(
            "AUDIT: {EventType} - UserId: {UserId}, ResourceType: {ResourceType}, ResourceId: {ResourceId}, Action: {Action}",
            eventType, userId, resourceType, resourceId, action);

        // Persist to database
        var auditLog = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Action = action,
            Details = $"Data access - ResourceType: {resourceType}, ResourceId: {resourceId}, Action: {action}",
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
