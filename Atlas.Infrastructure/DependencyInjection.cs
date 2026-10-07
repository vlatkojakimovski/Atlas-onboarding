namespace Atlas.Infrastructure;

using Atlas.Application.Services;
using Atlas.Domain.Configuration;
using Atlas.Domain.Repositories;
using Atlas.Infrastructure.Audit;
using Atlas.Infrastructure.Configuration;
using Atlas.Infrastructure.ExternalServices.CardOrdering;
using Atlas.Infrastructure.ExternalServices.CoreBanking;
using Atlas.Infrastructure.ExternalServices.IdentityVerification;
using Atlas.Infrastructure.ExternalServices.SanctionsScreening;
using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Services;
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
        services.Configure<MarketsConfiguration>(configuration);
        services.AddSingleton<IMarketConfigurationProvider, MarketConfigurationProvider>();

        // Register national ID validator (Task 4.2)
        // Singleton lifetime since it's a stateless validator with no side effects
        services.AddSingleton<INationalIdValidator, NationalIdValidator>();

        // Register application repository (Task 5.7)
        // Scoped lifetime to align with DbContext lifetime
        services.AddScoped<IApplicationRepository, ApplicationRepository>();

        // Register audit logger (Task 5.6)
        // Scoped lifetime to align with DbContext lifetime
        services.AddScoped<IAuditLogger, AuditLogger>();

        // Register external service adapters (Task 5.1-5.5)
        // Transient for mock services (stateless)
        services.AddTransient<IIdentityVerificationService, MockIdentityVerificationService>();
        services.AddTransient<ISanctionsScreeningService, MockSanctionsScreeningService>();
        services.AddScoped<ICoreBankingService, MockCoreBankingService>();
        services.AddScoped<ICardOrderingService, MockCardOrderingService>();

        // Register application orchestrator (Task 6.3)
        // Scoped lifetime
        services.AddScoped<IApplicationOrchestrator, ApplicationOrchestrator>();

        return services;
    }
}
