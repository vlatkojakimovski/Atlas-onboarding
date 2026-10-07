namespace Atlas.Infrastructure.ExternalServices.IdentityVerification;

using Atlas.Domain.ValueObjects;
using Atlas.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;

/// <summary>
/// Decorator that adds retry and timeout policies to identity verification service.
/// Implements requirements 11.1-11.4, 17.1-17.2.
/// </summary>
public class ResilientIdentityVerificationService : IIdentityVerificationService
{
    private readonly IIdentityVerificationService _innerService;
    private readonly ILogger<ResilientIdentityVerificationService> _logger;
    private readonly Polly.ResiliencePipeline<IdentityVerificationResult> _pipeline;

    public ResilientIdentityVerificationService(
        IIdentityVerificationService innerService,
        ILogger<ResilientIdentityVerificationService> logger)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pipeline = ResiliencePolicies.CreateIdentityVerificationPipeline<IdentityVerificationResult>(logger);
    }

    public async Task<IdentityVerificationResult> VerifyIdentityAsync(
        IdentityVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Executing identity verification with resilience policies");

        try
        {
            return await _pipeline.ExecuteAsync(
                async ct => await _innerService.VerifyIdentityAsync(request, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Identity verification failed after all retry attempts");
            throw;
        }
    }
}
