namespace Atlas.Infrastructure.ExternalServices.SanctionsScreening;

using Atlas.Domain.ValueObjects;
using Atlas.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;

/// <summary>
/// Decorator that adds retry and timeout policies to sanctions screening service.
/// Implements requirements 11.1-11.4, 17.1-17.2.
/// </summary>
public class ResilientSanctionsScreeningService : ISanctionsScreeningService
{
    private readonly ISanctionsScreeningService _innerService;
    private readonly ILogger<ResilientSanctionsScreeningService> _logger;
    private readonly Polly.ResiliencePipeline<SanctionsScreeningResult> _pipeline;

    public ResilientSanctionsScreeningService(
        ISanctionsScreeningService innerService,
        ILogger<ResilientSanctionsScreeningService> logger)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pipeline = ResiliencePolicies.CreateSanctionsScreeningPipeline<SanctionsScreeningResult>(logger);
    }

    public async Task<SanctionsScreeningResult> ScreenAsync(
        SanctionsScreeningRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Executing sanctions screening with resilience policies");

        try
        {
            return await _pipeline.ExecuteAsync(
                async ct => await _innerService.ScreenAsync(request, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sanctions screening failed after all retry attempts");
            throw;
        }
    }
}
