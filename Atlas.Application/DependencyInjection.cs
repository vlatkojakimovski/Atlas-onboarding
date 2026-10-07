namespace Atlas.Application;

using Atlas.Application.Handlers;
using Atlas.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering application layer services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all application layer services including validators and command handlers.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register validators (Task 6.1)
        services.AddScoped<IApplicationValidator, ApplicationValidator>();

        // Register command handlers (Task 6.2)
        services.AddScoped<SubmitApplicationCommandHandler>();

        // Note: IApplicationOrchestrator will be registered in Task 6.3

        return services;
    }
}
