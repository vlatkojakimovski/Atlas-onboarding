namespace Atlas.Domain.Repositories;

using Atlas.Domain.Entities;

/// <summary>
/// Defines the contract for persisting and retrieving Application aggregates.
/// Implements requirements 12.1-12.10.
/// </summary>
public interface IApplicationRepository
{
    /// <summary>
    /// Adds a new application to the repository.
    /// </summary>
    /// <param name="application">The application to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Application application, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an application by its unique identifier, including all related entities.
    /// </summary>
    /// <param name="id">The application ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application if found; otherwise, null.</returns>
    Task<Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing application in the repository.
    /// </summary>
    /// <param name="application">The application to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(Application application, CancellationToken cancellationToken = default);
}
