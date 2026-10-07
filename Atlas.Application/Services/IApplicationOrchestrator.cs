namespace Atlas.Application.Services;

using Atlas.Application.Models;
using Atlas.Domain.Entities;

/// <summary>
/// Defines the contract for orchestrating the complete application processing workflow.
/// Coordinates identity verification, sanctions screening, decision making, and downstream actions.
/// </summary>
public interface IApplicationOrchestrator
{
    /// <summary>
    /// Processes an application through the complete workflow.
    /// </summary>
    /// <param name="application">The application to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application result with final status and downstream identifiers.</returns>
    Task<ApplicationResult> ProcessApplicationAsync(
        Application application,
        CancellationToken cancellationToken = default);
}
