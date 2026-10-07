namespace Atlas.Domain;

using Atlas.Domain.BusinessLogic;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering domain services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all domain services including decision engine.
    /// </summary>
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // Register application decision engine (Task 4.3)
        // Singleton lifetime since it's a pure function with no side effects or state
        services.AddSingleton<IApplicationDecisionEngine, ApplicationDecisionEngine>();

        return services;
    }
}
