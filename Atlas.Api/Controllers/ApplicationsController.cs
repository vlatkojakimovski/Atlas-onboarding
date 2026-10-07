namespace Atlas.Api.Controllers;

using Atlas.Api.Contracts;
using Atlas.Application.Commands;
using Atlas.Application.Handlers;
using Atlas.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for managing customer onboarding applications.
/// Implements requirements 1.1, 1.7, 9.1-9.5, 13.1-13.10.
/// </summary>
[ApiController]
[Route("applications")]
public class ApplicationsController : ControllerBase
{
    private readonly SubmitApplicationCommandHandler _commandHandler;
    private readonly IApplicationRepository _repository;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(
        SubmitApplicationCommandHandler commandHandler,
        IApplicationRepository repository,
        ILogger<ApplicationsController> logger)
    {
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Submits a new customer onboarding application.
    /// POST /applications
    /// </summary>
    /// <param name="request">The application submission request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 Created with application ID and status.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitApplication(
        [FromBody] SubmitApplicationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Received application submission request for {FirstName} {LastName} in market {Market}",
            request.FirstName, request.LastName, request.Market);

        // Map API request to command
        var command = new SubmitApplicationCommand(
            FirstName: request.FirstName,
            LastName: request.LastName,
            DateOfBirth: request.DateOfBirth,
            Market: request.Market,
            NationalId: request.NationalId,
            Email: request.Email,
            Phone: request.Phone,
            Documents: request.Documents
                .Select(d => new DocumentSubmission(d.Type, d.Image))
                .ToList()
                .AsReadOnly(),
            TermsAccepted: request.TermsAccepted);

        // Handle the command
        var result = await _commandHandler.HandleAsync(command, cancellationToken);

        // Map to response
        var response = new ApplicationResponse
        {
            ApplicationId = result.ApplicationId,
            Status = result.Status.ToString()
        };

        _logger.LogInformation(
            "Application {ApplicationId} submitted successfully with status {Status}",
            result.ApplicationId, result.Status);

        // Return 201 Created with Location header
        return CreatedAtAction(
            nameof(GetApplication),
            new { id = result.ApplicationId },
            response);
    }

    /// <summary>
    /// Retrieves an application by ID.
    /// GET /applications/{id}
    /// </summary>
    /// <param name="id">The application ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with application details, or 404 Not Found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetApplication(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving application {ApplicationId}", id);

        // Retrieve application from repository
        var application = await _repository.GetByIdAsync(id, cancellationToken);

        if (application == null)
        {
            _logger.LogWarning("Application {ApplicationId} not found", id);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Application not found",
                Detail = $"Application with ID {id} was not found.",
                Instance = HttpContext.Request.Path
            });
        }

        // Map to response
        var response = new ApplicationDetailsResponse
        {
            ApplicationId = application.Id,
            Status = application.Status.ToString(),
            FirstName = application.FirstName,
            LastName = application.LastName,
            DateOfBirth = application.DateOfBirth.ToDateTime(TimeOnly.MinValue),
            Market = application.MarketCode,
            Email = application.Email,
            Phone = application.Phone,
            CreatedAt = application.CreatedAt,
            CompletedAt = application.CompletedAt,
            AccountId = application.AccountDetails?.AccountId,
            RejectionReasons = application.Decision?.RejectionReasons
                .Select(r => r.ToString())
                .ToList()
        };

        _logger.LogDebug("Application {ApplicationId} retrieved successfully", id);

        return Ok(response);
    }
}
