namespace Atlas.Infrastructure.Repositories;

using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Repositories;
using Atlas.Infrastructure.Data;
using Atlas.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation of application repository using EF Core for persistence.
/// Maps between domain Application and ApplicationEntity.
/// Implements requirements 12.1-12.10.
/// </summary>
public class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ApplicationRepository> _logger;

    public ApplicationRepository(
        ApplicationDbContext dbContext,
        ILogger<ApplicationRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Adds a new application to the database.
    /// </summary>
    public async Task AddAsync(Application application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);

        _logger.LogDebug("Adding application {ApplicationId} to repository", application.Id);

        var entity = MapToEntity(application);
        await _dbContext.Applications.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Application {ApplicationId} added successfully", application.Id);
    }

    /// <summary>
    /// Retrieves an application by ID with all related entities eagerly loaded.
    /// </summary>
    public async Task<Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving application {ApplicationId} from repository", id);

        var entity = await _dbContext.Applications
            .Include(a => a.Documents)
            .Include(a => a.IdentityVerification)
            .Include(a => a.SanctionsScreening)
                .ThenInclude(s => s!.Matches)
            .Include(a => a.AccountDetails)
            .Include(a => a.CardOrder)
            .Include(a => a.Decision)
                .ThenInclude(d => d!.RejectionReasons)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Application {ApplicationId} not found", id);
            return null;
        }

        _logger.LogDebug("Application {ApplicationId} retrieved successfully", id);
        return MapToDomain(entity);
    }

    /// <summary>
    /// Updates an existing application in the database.
    /// </summary>
    public async Task UpdateAsync(Application application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);

        _logger.LogDebug("Updating application {ApplicationId} in repository", application.Id);

        var existingEntity = await _dbContext.Applications
            .Include(a => a.Documents)
            .Include(a => a.IdentityVerification)
            .Include(a => a.SanctionsScreening)
                .ThenInclude(s => s!.Matches)
            .Include(a => a.AccountDetails)
            .Include(a => a.CardOrder)
            .Include(a => a.Decision)
                .ThenInclude(d => d!.RejectionReasons)
            .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);

        if (existingEntity == null)
        {
            throw new InvalidOperationException($"Application {application.Id} not found for update.");
        }

        // Update the entity with domain model values
        UpdateEntity(existingEntity, application);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Application {ApplicationId} updated successfully", application.Id);
    }

    /// <summary>
    /// Maps domain Application to ApplicationEntity for persistence.
    /// </summary>
    private ApplicationEntity MapToEntity(Application domain)
    {
        var entity = new ApplicationEntity
        {
            Id = domain.Id,
            FirstName = domain.FirstName,
            LastName = domain.LastName,
            DateOfBirth = domain.DateOfBirth,
            MarketCode = domain.MarketCode,
            NationalId = domain.NationalId,
            Email = domain.Email,
            Phone = domain.Phone,
            Status = domain.Status.ToString(),
            CreatedAt = domain.CreatedAt,
            CompletedAt = domain.CompletedAt
        };

        // Map documents
        foreach (var doc in domain.Documents)
        {
            entity.Documents.Add(new DocumentEntity
            {
                Id = doc.Id,
                ApplicationId = domain.Id,
                DocumentType = doc.Type.ToString(),
                ImageData = doc.ImageData,
                UploadedAt = doc.UploadedAt
            });
        }

        // Map identity verification if exists
        if (domain.IdentityVerification != null)
        {
            entity.IdentityVerification = new IdentityVerificationEntity
            {
                Id = domain.IdentityVerification.Id,
                ApplicationId = domain.Id,
                ProviderId = domain.IdentityVerification.ProviderId,
                DocumentStatus = domain.IdentityVerification.DocumentStatus.ToString(),
                FaceMatch = domain.IdentityVerification.FaceMatch,
                Confidence = domain.IdentityVerification.Confidence,
                VerifiedAt = domain.IdentityVerification.VerifiedAt
            };
        }

        // Map sanctions screening if exists
        if (domain.SanctionsScreening != null)
        {
            var screeningEntity = new SanctionsScreeningEntity
            {
                Id = domain.SanctionsScreening.Id,
                ApplicationId = domain.Id,
                CaseId = domain.SanctionsScreening.CaseId,
                Status = domain.SanctionsScreening.Status.ToString(),
                ScreenedAt = domain.SanctionsScreening.ScreenedAt
            };

            foreach (var match in domain.SanctionsScreening.Matches)
            {
                screeningEntity.Matches.Add(new SanctionMatchEntity
                {
                    Id = match.Id,
                    ScreeningId = domain.SanctionsScreening.Id,
                    MatchType = match.Type.ToString(),
                    Score = match.Score,
                    Subject = match.Subject
                });
            }

            entity.SanctionsScreening = screeningEntity;
        }

        // Map account details if exists
        if (domain.AccountDetails != null)
        {
            entity.AccountDetails = new AccountDetailsEntity
            {
                Id = domain.AccountDetails.Id,
                ApplicationId = domain.Id,
                AccountId = domain.AccountDetails.AccountId,
                AccountNumber = domain.AccountDetails.AccountNumber,
                CreatedAt = domain.AccountDetails.CreatedAt
            };
        }

        // Map card order if exists
        if (domain.CardOrder != null)
        {
            entity.CardOrder = new CardOrderEntity
            {
                Id = domain.CardOrder.Id,
                ApplicationId = domain.Id,
                AccountId = domain.CardOrder.AccountId,
                CardOrderId = domain.CardOrder.CardOrderId,
                CardReference = domain.CardOrder.CardReference,
                RequiresBranchActivation = domain.CardOrder.RequiresBranchActivation,
                OrderedAt = domain.CardOrder.OrderedAt
            };
        }

        // Map decision if exists
        if (domain.Decision != null)
        {
            var decisionEntity = new ApplicationDecisionEntity
            {
                Id = domain.Decision.Id,
                ApplicationId = domain.Id,
                Status = domain.Decision.Status.ToString(),
                DecidedAt = domain.Decision.DecidedAt
            };

            foreach (var reason in domain.Decision.RejectionReasons)
            {
                decisionEntity.RejectionReasons.Add(new RejectionReasonEntity
                {
                    Id = Guid.NewGuid(),
                    DecisionId = domain.Decision.Id,
                    Reason = reason.ToString()
                });
            }

            entity.Decision = decisionEntity;
        }

        return entity;
    }

    /// <summary>
    /// Maps ApplicationEntity to domain Application for retrieval.
    /// </summary>
    private Application MapToDomain(ApplicationEntity entity)
    {
        // Create application using reflection to bypass constructor validation
        // This is necessary because we're reconstructing from database
        var application = (Application)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Application));

        // Set properties using reflection
        SetProperty(application, nameof(Application.Id), entity.Id);
        SetProperty(application, nameof(Application.FirstName), entity.FirstName);
        SetProperty(application, nameof(Application.LastName), entity.LastName);
        SetProperty(application, nameof(Application.DateOfBirth), entity.DateOfBirth);
        SetProperty(application, nameof(Application.MarketCode), entity.MarketCode);
        SetProperty(application, nameof(Application.NationalId), entity.NationalId);
        SetProperty(application, nameof(Application.Email), entity.Email);
        SetProperty(application, nameof(Application.Phone), entity.Phone);
        SetProperty(application, nameof(Application.Status), Enum.Parse<ApplicationStatus>(entity.Status));
        SetProperty(application, nameof(Application.CreatedAt), entity.CreatedAt);
        SetProperty(application, nameof(Application.CompletedAt), entity.CompletedAt);

        // Map documents (initialize collection first)
        var documents = new List<Document>();
        foreach (var docEntity in entity.Documents)
        {
            var doc = (Document)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(Document));
            SetProperty(doc, nameof(Document.Id), docEntity.Id);
            SetProperty(doc, nameof(Document.ApplicationId), docEntity.ApplicationId);
            SetProperty(doc, nameof(Document.Type), Enum.Parse<DocumentType>(docEntity.DocumentType));
            SetProperty(doc, nameof(Document.ImageData), docEntity.ImageData);
            SetProperty(doc, nameof(Document.UploadedAt), docEntity.UploadedAt);
            documents.Add(doc);
        }
        SetProperty(application, nameof(Application.Documents), documents);

        // Map identity verification if exists
        if (entity.IdentityVerification != null)
        {
            var iv = (IdentityVerification)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(IdentityVerification));
            SetProperty(iv, nameof(IdentityVerification.Id), entity.IdentityVerification.Id);
            SetProperty(iv, nameof(IdentityVerification.ApplicationId), entity.IdentityVerification.ApplicationId);
            SetProperty(iv, nameof(IdentityVerification.ProviderId), entity.IdentityVerification.ProviderId);
            SetProperty(iv, nameof(IdentityVerification.DocumentStatus), Enum.Parse<DocumentVerificationStatus>(entity.IdentityVerification.DocumentStatus));
            SetProperty(iv, nameof(IdentityVerification.FaceMatch), entity.IdentityVerification.FaceMatch);
            SetProperty(iv, nameof(IdentityVerification.Confidence), entity.IdentityVerification.Confidence);
            SetProperty(iv, nameof(IdentityVerification.VerifiedAt), entity.IdentityVerification.VerifiedAt);
            SetProperty(application, nameof(Application.IdentityVerification), iv);
        }

        // Map sanctions screening if exists
        if (entity.SanctionsScreening != null)
        {
            var ss = (SanctionsScreening)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(SanctionsScreening));
            SetProperty(ss, nameof(SanctionsScreening.Id), entity.SanctionsScreening.Id);
            SetProperty(ss, nameof(SanctionsScreening.ApplicationId), entity.SanctionsScreening.ApplicationId);
            SetProperty(ss, nameof(SanctionsScreening.CaseId), entity.SanctionsScreening.CaseId);
            SetProperty(ss, nameof(SanctionsScreening.Status), Enum.Parse<ScreeningStatus>(entity.SanctionsScreening.Status));
            SetProperty(ss, nameof(SanctionsScreening.ScreenedAt), entity.SanctionsScreening.ScreenedAt);

            var matches = new List<SanctionMatch>();
            foreach (var matchEntity in entity.SanctionsScreening.Matches)
            {
                var match = (SanctionMatch)System.Runtime.Serialization.FormatterServices
                    .GetUninitializedObject(typeof(SanctionMatch));
                SetProperty(match, nameof(SanctionMatch.Id), matchEntity.Id);
                SetProperty(match, nameof(SanctionMatch.Type), Enum.Parse<MatchType>(matchEntity.MatchType));
                SetProperty(match, nameof(SanctionMatch.Score), matchEntity.Score);
                SetProperty(match, nameof(SanctionMatch.Subject), matchEntity.Subject);
                matches.Add(match);
            }
            SetProperty(ss, nameof(SanctionsScreening.Matches), matches);
            SetProperty(application, nameof(Application.SanctionsScreening), ss);
        }

        // Map account details if exists
        if (entity.AccountDetails != null)
        {
            var ad = (AccountDetails)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(AccountDetails));
            SetProperty(ad, nameof(AccountDetails.Id), entity.AccountDetails.Id);
            SetProperty(ad, nameof(AccountDetails.ApplicationId), entity.AccountDetails.ApplicationId);
            SetProperty(ad, nameof(AccountDetails.AccountId), entity.AccountDetails.AccountId);
            SetProperty(ad, nameof(AccountDetails.AccountNumber), entity.AccountDetails.AccountNumber);
            SetProperty(ad, nameof(AccountDetails.CreatedAt), entity.AccountDetails.CreatedAt);
            SetProperty(application, nameof(Application.AccountDetails), ad);
        }

        // Map card order if exists
        if (entity.CardOrder != null)
        {
            var co = (CardOrder)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(CardOrder));
            SetProperty(co, nameof(CardOrder.Id), entity.CardOrder.Id);
            SetProperty(co, nameof(CardOrder.ApplicationId), entity.CardOrder.ApplicationId);
            SetProperty(co, nameof(CardOrder.AccountId), entity.CardOrder.AccountId);
            SetProperty(co, nameof(CardOrder.CardOrderId), entity.CardOrder.CardOrderId);
            SetProperty(co, nameof(CardOrder.CardReference), entity.CardOrder.CardReference);
            SetProperty(co, nameof(CardOrder.RequiresBranchActivation), entity.CardOrder.RequiresBranchActivation);
            SetProperty(co, nameof(CardOrder.OrderedAt), entity.CardOrder.OrderedAt);
            SetProperty(application, nameof(Application.CardOrder), co);
        }

        // Map decision if exists
        if (entity.Decision != null)
        {
            var dec = (ApplicationDecision)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(ApplicationDecision));
            SetProperty(dec, nameof(ApplicationDecision.Id), entity.Decision.Id);
            SetProperty(dec, nameof(ApplicationDecision.ApplicationId), entity.Decision.ApplicationId);
            SetProperty(dec, nameof(ApplicationDecision.Status), Enum.Parse<ApplicationStatus>(entity.Decision.Status));
            SetProperty(dec, nameof(ApplicationDecision.DecidedAt), entity.Decision.DecidedAt);

            var reasons = entity.Decision.RejectionReasons
                .Select(r => Enum.Parse<RejectionReason>(r.Reason))
                .ToList();
            SetProperty(dec, nameof(ApplicationDecision.RejectionReasons), reasons);
            SetProperty(application, nameof(Application.Decision), dec);
        }

        return application;
    }

    /// <summary>
    /// Updates existing entity with values from domain model.
    /// </summary>
    private void UpdateEntity(ApplicationEntity entity, Application domain)
    {
        // Update scalar properties
        entity.Status = domain.Status.ToString();
        entity.CompletedAt = domain.CompletedAt;

        // Update or add identity verification
        if (domain.IdentityVerification != null)
        {
            if (entity.IdentityVerification == null)
            {
                entity.IdentityVerification = new IdentityVerificationEntity
                {
                    Id = domain.IdentityVerification.Id,
                    ApplicationId = domain.Id
                };
            }
            entity.IdentityVerification.ProviderId = domain.IdentityVerification.ProviderId;
            entity.IdentityVerification.DocumentStatus = domain.IdentityVerification.DocumentStatus.ToString();
            entity.IdentityVerification.FaceMatch = domain.IdentityVerification.FaceMatch;
            entity.IdentityVerification.Confidence = domain.IdentityVerification.Confidence;
            entity.IdentityVerification.VerifiedAt = domain.IdentityVerification.VerifiedAt;
        }

        // Update or add sanctions screening
        if (domain.SanctionsScreening != null)
        {
            if (entity.SanctionsScreening == null)
            {
                entity.SanctionsScreening = new SanctionsScreeningEntity
                {
                    Id = domain.SanctionsScreening.Id,
                    ApplicationId = domain.Id
                };
            }
            entity.SanctionsScreening.CaseId = domain.SanctionsScreening.CaseId;
            entity.SanctionsScreening.Status = domain.SanctionsScreening.Status.ToString();
            entity.SanctionsScreening.ScreenedAt = domain.SanctionsScreening.ScreenedAt;

            // Update matches
            entity.SanctionsScreening.Matches.Clear();
            foreach (var match in domain.SanctionsScreening.Matches)
            {
                entity.SanctionsScreening.Matches.Add(new SanctionMatchEntity
                {
                    Id = match.Id,
                    ScreeningId = domain.SanctionsScreening.Id,
                    MatchType = match.Type.ToString(),
                    Score = match.Score,
                    Subject = match.Subject
                });
            }
        }

        // Update or add account details
        if (domain.AccountDetails != null)
        {
            if (entity.AccountDetails == null)
            {
                entity.AccountDetails = new AccountDetailsEntity
                {
                    Id = domain.AccountDetails.Id,
                    ApplicationId = domain.Id
                };
            }
            entity.AccountDetails.AccountId = domain.AccountDetails.AccountId;
            entity.AccountDetails.AccountNumber = domain.AccountDetails.AccountNumber;
            entity.AccountDetails.CreatedAt = domain.AccountDetails.CreatedAt;
        }

        // Update or add card order
        if (domain.CardOrder != null)
        {
            if (entity.CardOrder == null)
            {
                entity.CardOrder = new CardOrderEntity
                {
                    Id = domain.CardOrder.Id,
                    ApplicationId = domain.Id
                };
            }
            entity.CardOrder.AccountId = domain.CardOrder.AccountId;
            entity.CardOrder.CardOrderId = domain.CardOrder.CardOrderId;
            entity.CardOrder.CardReference = domain.CardOrder.CardReference;
            entity.CardOrder.RequiresBranchActivation = domain.CardOrder.RequiresBranchActivation;
            entity.CardOrder.OrderedAt = domain.CardOrder.OrderedAt;
        }

        // Update or add decision
        if (domain.Decision != null)
        {
            if (entity.Decision == null)
            {
                entity.Decision = new ApplicationDecisionEntity
                {
                    Id = domain.Decision.Id,
                    ApplicationId = domain.Id
                };
            }
            entity.Decision.Status = domain.Decision.Status.ToString();
            entity.Decision.DecidedAt = domain.Decision.DecidedAt;

            // Update rejection reasons
            entity.Decision.RejectionReasons.Clear();
            foreach (var reason in domain.Decision.RejectionReasons)
            {
                entity.Decision.RejectionReasons.Add(new RejectionReasonEntity
                {
                    Id = Guid.NewGuid(),
                    DecisionId = domain.Decision.Id,
                    Reason = reason.ToString()
                });
            }
        }
    }

    /// <summary>
    /// Helper method to set private properties using reflection.
    /// </summary>
    private void SetProperty(object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property != null)
        {
            property.SetValue(obj, value);
        }
    }
}
