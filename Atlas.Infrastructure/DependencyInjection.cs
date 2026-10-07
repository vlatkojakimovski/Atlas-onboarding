namespace Atlas.Infrastructure;

using Atlas.Domain.Configuration;
using Atlas.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering infrastructure services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all infrastructure services including market configuration provider.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register market configuration provider (Task 4.1)
        // Uses IOptionsMonitor for hot-reload support (Requirement 16.5)
        services.Configure<MarketsConfiguration>(configuration.GetSection("Markets"));
        services.AddSingleton<IMarketConfigurationProvider, MarketConfigurationProvider>();

        // Register national ID validator (Task 4.2)
        // Singleton lifetime since it's a stateless validator with no side effects
        services.AddSingleton<INationalIdValidator, NationalIdValidator>();

        return services;
    }
}
