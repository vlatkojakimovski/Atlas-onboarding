namespace Atlas.Application.Handlers;

using Atlas.Application.Commands;
using Atlas.Application.Exceptions;
using Atlas.Application.Models;
using Atlas.Application.Services;
using Atlas.Application.Validation;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Repositories;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handles the submission of new customer onboarding applications.
/// Implements requirements 1.1-1.8, 8.5, 10.1.
/// </summary>
public class SubmitApplicationCommandHandler
{
    private readonly IApplicationValidator _validator;
    private readonly IApplicationRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly IApplicationOrchestrator _orchestrator;
    private readonly ILogger<SubmitApplicationCommandHandler> _logger;

    public SubmitApplicationCommandHandler(
        IApplicationValidator validator,
        IApplicationRepository repository,
        IAuditLogger auditLogger,
        IApplicationOrchestrator orchestrator,
        ILogger<SubmitApplicationCommandHandler> logger)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the application submission command.
    /// </summary>
    public async Task<ApplicationResult> HandleAsync(
        SubmitApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing application submission for {FirstName} {LastName} in market {Market}",
            command.FirstName, command.LastName, command.Market);

        // Step 1: Validate the command
        var validationResult = _validator.Validate(command);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning(
                "Application validation failed with {ErrorCount} errors",
                validationResult.Errors.Count);

            throw new ValidationException(validationResult.Errors);
        }

        _logger.LogDebug("Application validation passed");

        // Step 2: Create Application aggregate with status Pending
        var application = new Application(
            firstName: command.FirstName,
            lastName: command.LastName,
            dateOfBirth: DateOnly.FromDateTime(command.DateOfBirth),
            marketCode: command.Market,
            nationalId: command.NationalId,
            email: command.Email,
            phone: command.Phone);

        // Add documents to application
        foreach (var doc in command.Documents)
        {
            var documentType = Enum.Parse<DocumentType>(doc.Type, ignoreCase: true);
            var imageBytes = Convert.FromBase64String(doc.Image);
            application.AddDocument(documentType, imageBytes);
        }

        _logger.LogDebug(
            "Application aggregate created with ID {ApplicationId}",
            application.Id);

        // Step 3: Persist initial application state (Pending with documents only)
        await _repository.AddAsync(application, cancellationToken);

        _logger.LogInformation(
            "Application {ApplicationId} persisted to database with Pending status",
            application.Id);

        // NOTE: At this point, the application entity is saved but the domain object
        // continues to be used by the orchestrator for further processing

        // Step 4: Log application created event
        await _auditLogger.LogApplicationCreatedAsync(
            application.Id,
            command.Market,
            userId: "system", // TODO: Get from authentication context when implemented
            cancellationToken);

        _logger.LogDebug(
            "Application created event logged for {ApplicationId}",
            application.Id);

        // Step 5: Invoke orchestrator to process application and update it
        _logger.LogInformation(
            "Starting application processing workflow for {ApplicationId}",
            application.Id);

        ApplicationResult result = await _orchestrator.ProcessApplicationAsync(application, cancellationToken);

            _logger.LogInformation(
                "Application {ApplicationId} processing completed with status {Status}",
                result.ApplicationId, result.Status);

        // Step 6: Return result
        return result;
    }
}
