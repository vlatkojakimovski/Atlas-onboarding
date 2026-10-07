namespace Atlas.Infrastructure.Resilience;

using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;

/// <summary>
/// Provides Polly resilience policies for external service calls.
/// Implements requirements 11.1-11.4, 17.1-17.2.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Creates a retry pipeline for identity verification service.
    /// 3 retries with exponential backoff (2s, 4s, 8s).
    /// </summary>
    public static ResiliencePipeline<T> CreateIdentityVerificationPipeline<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(TimeSpan.FromSeconds(10)) // 10-second timeout per request
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential, // 2s, 4s, 8s
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Identity verification service retry attempt {AttemptNumber} after {RetryDelay}ms delay. Exception: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Creates a retry pipeline for sanctions screening service.
    /// 3 retries with exponential backoff (2s, 4s, 8s).
    /// </summary>
    public static ResiliencePipeline<T> CreateSanctionsScreeningPipeline<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(TimeSpan.FromSeconds(10)) // 10-second timeout per request
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential, // 2s, 4s, 8s
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Sanctions screening service retry attempt {AttemptNumber} after {RetryDelay}ms delay. Exception: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Creates a retry pipeline for core banking service.
    /// 3 retries with exponential backoff (2s, 4s, 8s).
    /// </summary>
    public static ResiliencePipeline<T> CreateCoreBankingPipeline<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(TimeSpan.FromSeconds(10)) // 10-second timeout per request
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential, // 2s, 4s, 8s
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Core banking service retry attempt {AttemptNumber} after {RetryDelay}ms delay. Exception: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Creates a retry pipeline for card ordering service.
    /// 3 retries with exponential backoff (2s, 4s, 8s).
    /// </summary>
    public static ResiliencePipeline<T> CreateCardOrderingPipeline<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(TimeSpan.FromSeconds(10)) // 10-second timeout per request
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential, // 2s, 4s, 8s
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Card ordering service retry attempt {AttemptNumber} after {RetryDelay}ms delay. Exception: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }
}
