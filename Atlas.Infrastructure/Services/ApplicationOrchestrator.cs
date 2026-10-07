namespace Atlas.Infrastructure.Services;

using Atlas.Application.Models;
using Atlas.Application.Services;
using Atlas.Domain.BusinessLogic;
using Atlas.Domain.Configuration;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Repositories;
using Atlas.Domain.ValueObjects;
using Atlas.Infrastructure.ExternalServices.CardOrdering;
using Atlas.Infrastructure.ExternalServices.CoreBanking;
using Atlas.Infrastructure.ExternalServices.IdentityVerification;
using Atlas.Infrastructure.ExternalServices.SanctionsScreening;
using Microsoft.Extensions.Logging;

/// <summary>
/// Orchestrates the complete application processing workflow.
/// Coordinates identity verification, sanctions screening, decision making, and downstream actions.
/// Implements requirements 5.1-5.7, 6.1-6.5, 7.1-7.6, 8.1-8.5, 10.2-10.7.
/// </summary>
public class ApplicationOrchestrator : IApplicationOrchestrator
{
    private readonly IIdentityVerificationService _verificationService;
    private readonly ISanctionsScreeningService _screeningService;
    private readonly IApplicationDecisionEngine _decisionEngine;
    private readonly ICoreBankingService _coreBankingService;
    private readonly ICardOrderingService _cardOrderingService;
    private readonly IApplicationRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly IMarketConfigurationProvider _marketConfigProvider;
    private readonly ILogger<ApplicationOrchestrator> _logger;

    public ApplicationOrchestrator(
        IIdentityVerificationService verificationService,
        ISanctionsScreeningService screeningService,
        IApplicationDecisionEngine decisionEngine,
        ICoreBankingService coreBankingService,
        ICardOrderingService cardOrderingService,
        IApplicationRepository repository,
        IAuditLogger auditLogger,
        IMarketConfigurationProvider marketConfigProvider,
        ILogger<ApplicationOrchestrator> logger)
    {
        _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
        _screeningService = screeningService ?? throw new ArgumentNullException(nameof(screeningService));
        _decisionEngine = decisionEngine ?? throw new ArgumentNullException(nameof(decisionEngine));
        _coreBankingService = coreBankingService ?? throw new ArgumentNullException(nameof(coreBankingService));
        _cardOrderingService = cardOrderingService ?? throw new ArgumentNullException(nameof(cardOrderingService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        _marketConfigProvider = marketConfigProvider ?? throw new ArgumentNullException(nameof(marketConfigProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes an application through the complete workflow.
    /// </summary>
    public async Task<ApplicationResult> ProcessApplicationAsync(
        Application application,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(application);

            _logger.LogInformation(
                "Starting application processing for {ApplicationId}",
                application.Id);

            // Get market configuration
            var marketConfig = _marketConfigProvider.GetMarketConfiguration(application.MarketCode);
            if (marketConfig == null)
            {
                throw new InvalidOperationException($"Market configuration not found for market: {application.MarketCode}");
            }

            // Step 1: Execute identity verification and sanctions screening in parallel
            _logger.LogInformation(
                "Executing identity verification and sanctions screening in parallel for {ApplicationId}",
                application.Id);

            var verificationTask = ExecuteIdentityVerificationAsync(application, cancellationToken);
            var screeningTask = ExecuteSanctionsScreeningAsync(application, cancellationToken);

            await Task.WhenAll(verificationTask, screeningTask);

            var verificationResult = await verificationTask;
            var screeningResult = await screeningTask;
            
            // Step 2: Log verification and screening completion
            await _auditLogger.LogVerificationCompletedAsync(
                application.Id,
                verificationResult,
                cancellationToken);

            await _auditLogger.LogScreeningCompletedAsync(
                application.Id,
                screeningResult,
                cancellationToken);

            _logger.LogInformation(
                "Verification and screening completed for {ApplicationId}. DocumentStatus: {DocumentStatus}, FaceMatch: {FaceMatch}, ScreeningStatus: {ScreeningStatus}",
                application.Id, verificationResult.DocumentStatus, verificationResult.FaceMatch, screeningResult.Status);

            // Update application with verification and screening results
            application.CompleteVerification(verificationResult);
            application.CompleteScreening(screeningResult);

            // Step 3: Invoke decision engine
            _logger.LogDebug(
                "Invoking decision engine for {ApplicationId}",
                application.Id);

            Atlas.Domain.ValueObjects.ApplicationDecision decision = _decisionEngine.MakeDecision(
                verificationResult,
                screeningResult,
                marketConfig);

            // Step 4: Log decision made
            await _auditLogger.LogDecisionMadeAsync(
                application.Id,
                decision,
                cancellationToken);

            _logger.LogInformation(
                "Decision made for {ApplicationId}: {Status}, IsApproved: {IsApproved}, ReasonCount: {ReasonCount}",
                application.Id, decision.Status, decision.IsApproved, decision.Reasons.Count);

            // Step 5 & 6: Process approval or rejection
            ApplicationResult result;

            if (decision.IsApproved)
            {
                _logger.LogDebug("Processing approval workflow");
                result = await ProcessApprovalAsync(application, marketConfig, cancellationToken);
            }
            else
            {
                result = await ProcessRejectionAsync(application, decision, cancellationToken);
            }

            // Step 7: Log application status changed
            await _auditLogger.LogApplicationStatusChangedAsync(
                application.Id,
                ApplicationStatus.Pending,
                result.Status,
                GetStatusChangeReason(result),
                cancellationToken);

            // Step 8: Update the application with all changes
            await _repository.UpdateAsync(application, cancellationToken);

            _logger.LogInformation(
                "Application processing completed for {ApplicationId} with status {Status}",
                application.Id, result.Status);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "CRITICAL ERROR in orchestrator for {ApplicationId}. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                application.Id, ex.GetType().Name, ex.Message, ex.StackTrace);
            throw;
        }
    }

    /// <summary>
    /// Executes identity verification for the application.
    /// </summary>
    private async Task<IdentityVerificationResult> ExecuteIdentityVerificationAsync(
        Application application,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting identity verification for {ApplicationId}", application.Id);

        // Get the first identity document (PASSPORT or ID_CARD)
        var idDocument = application.Documents
            .FirstOrDefault(d => d.Type == DocumentType.PASSPORT || d.Type == DocumentType.ID_CARD);

        // Get the selfie document
        var selfie = application.Documents
            .FirstOrDefault(d => d.Type == DocumentType.SELFIE);

        if (idDocument == null || selfie == null)
        {
            throw new InvalidOperationException("Identity documents are missing");
        }

        var request = new IdentityVerificationRequest(
            DocumentType: idDocument.Type,
            DocumentImageBase64: Convert.ToBase64String(idDocument.ImageData),
            SelfieImageBase64: Convert.ToBase64String(selfie.ImageData));

        return await _verificationService.VerifyIdentityAsync(request, cancellationToken);
    }

    /// <summary>
    /// Executes sanctions screening for the application.
    /// </summary>
    private async Task<SanctionsScreeningResult> ExecuteSanctionsScreeningAsync(
        Application application,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting sanctions screening for {ApplicationId}", application.Id);

        var request = new SanctionsScreeningRequest(
            FirstName: application.FirstName,
            LastName: application.LastName,
            DateOfBirth: application.DateOfBirth.ToDateTime(TimeOnly.MinValue),
            Nationality: application.MarketCode);

        return await _screeningService.ScreenAsync(request, cancellationToken);
    }

    /// <summary>
    /// Processes an approved application by opening account and ordering card.
    /// </summary>
    private async Task<ApplicationResult> ProcessApprovalAsync(
        Application application,
        MarketConfiguration marketConfig,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing approval for {ApplicationId}",
            application.Id);

        // Open account via Core Banking Service
        var accountRequest = new AccountCreationRequest(
            ApplicationId: application.Id,
            FirstName: application.FirstName,
            LastName: application.LastName,
            DateOfBirth: application.DateOfBirth.ToDateTime(TimeOnly.MinValue),
            NationalId: application.NationalId,
            Email: application.Email,
            Phone: application.Phone,
            MarketCode: application.MarketCode);

        var accountResult = await _coreBankingService.OpenAccountAsync(accountRequest, cancellationToken);

        _logger.LogInformation(
            "Account opened for {ApplicationId}: AccountId={AccountId}, AccountNumber={AccountNumber}",
            application.Id, accountResult.AccountId, accountResult.AccountNumber);

        // Log account opened
        await _auditLogger.LogAccountOpenedAsync(
            application.Id,
            accountResult.AccountId,
            cancellationToken);

        // Order card via Card Ordering Service (set RequiresBranchActivation for MD market)
        var cardRequest = new CardOrderRequest(
            AccountId: accountResult.AccountId,
            FirstName: application.FirstName,
            LastName: application.LastName,
            DeliveryAddress: $"{application.Email}",  // Simplified address
            RequiresBranchActivation: marketConfig.RequiresBranchActivation);

        var cardResult = await _cardOrderingService.OrderCardAsync(cardRequest, cancellationToken);

        _logger.LogInformation(
            "Card ordered for {ApplicationId}: CardOrderId={CardOrderId}, CardReference={CardReference}, RequiresBranchActivation={RequiresBranchActivation}",
            application.Id, cardResult.CardOrderId, cardResult.CardReference, marketConfig.RequiresBranchActivation);

        // Log card ordered
        await _auditLogger.LogCardOrderedAsync(
            application.Id,
            cardResult.CardOrderId,
            cancellationToken);

        // Update application status to APPROVED
        application.Approve(
            accountId: accountResult.AccountId,
            accountNumber: accountResult.AccountNumber,
            cardOrderId: cardResult.CardOrderId,
            cardReference: cardResult.CardReference,
            requiresBranchActivation: marketConfig.RequiresBranchActivation);

        return new ApplicationResult(
            ApplicationId: application.Id,
            Status: ApplicationStatus.Approved,
            RejectionReasons: Array.Empty<RejectionReason>(),
            AccountId: accountResult.AccountId,
            CardOrderId: cardResult.CardOrderId);
    }

    /// <summary>
    /// Processes a rejected application.
    /// </summary>
    private async Task<ApplicationResult> ProcessRejectionAsync(
        Application application,
        Atlas.Domain.ValueObjects.ApplicationDecision decision,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing rejection for {ApplicationId} with {ReasonCount} reasons",
            application.Id, decision.Reasons.Count);

        // Update application status to REJECTED with reasons
        application.Reject(decision.Reasons);

        return new ApplicationResult(
            ApplicationId: application.Id,
            Status: ApplicationStatus.Rejected,
            RejectionReasons: decision.Reasons,
            AccountId: null,
            CardOrderId: null);
    }

    /// <summary>
    /// Gets a human-readable reason for status change.
    /// </summary>
    private string GetStatusChangeReason(ApplicationResult result)
    {
        if (result.Status == ApplicationStatus.Approved)
        {
            return "Application approved - all verification checks passed";
        }
        else if (result.Status == ApplicationStatus.Rejected && result.RejectionReasons.Any())
        {
            return $"Application rejected - Reasons: {string.Join(", ", result.RejectionReasons)}";
        }
        else
        {
            return $"Application status changed to {result.Status}";
        }
    }
}
