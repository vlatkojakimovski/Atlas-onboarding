namespace Atlas.Infrastructure.ExternalServices.IdentityVerification;

using Atlas.Domain.Enums;
using Atlas.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Mock implementation of identity verification service for development and testing.
/// Simulates various verification scenarios based on configuration.
/// Implements requirements 3.1-3.6.
/// </summary>
public class MockIdentityVerificationService : IIdentityVerificationService
{
    private readonly ILogger<MockIdentityVerificationService> _logger;
    private readonly IConfiguration _configuration;

    public MockIdentityVerificationService(
        ILogger<MockIdentityVerificationService> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Simulates identity verification with configurable test scenarios.
    /// </summary>
    public async Task<IdentityVerificationResult> VerifyIdentityAsync(
        IdentityVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Starting mock identity verification for document type: {DocumentType}",
            request.DocumentType);

        // Get mock scenario from configuration or use default (happy path)
        var mockScenario = _configuration["MockServices:IdentityVerification:Scenario"] ?? "HappyPath";
        var delayMs = _configuration.GetValue<int>("MockServices:IdentityVerification:DelayMs", 500);

        _logger.LogDebug(
            "Using mock scenario: {Scenario} with delay: {DelayMs}ms",
            mockScenario, delayMs);

        // Simulate network delay
        await Task.Delay(delayMs, cancellationToken);

        // Generate mock provider ID
        var providerId = $"IDNOW-{Guid.NewGuid():N}";

        IdentityVerificationResult result = mockScenario.ToLowerInvariant() switch
        {
            "happypath" or "valid" => CreateHappyPathResult(providerId),
            "invaliddocument" => CreateInvalidDocumentResult(providerId),
            "facemismatch" => CreateFaceMismatchResult(providerId),
            "inconclusive" => CreateInconclusiveResult(providerId),
            "timeout" => throw new TaskCanceledException("Mock identity verification service timeout"),
            _ => CreateHappyPathResult(providerId) // Default to happy path
        };

        _logger.LogInformation(
            "Mock identity verification completed. ProviderId: {ProviderId}, DocumentStatus: {DocumentStatus}, FaceMatch: {FaceMatch}, Confidence: {Confidence}",
            result.ProviderId, result.DocumentStatus, result.FaceMatch, result.Confidence);

        return result;
    }

    /// <summary>
    /// Creates a happy path result: Valid document + face match + high confidence.
    /// </summary>
    private static IdentityVerificationResult CreateHappyPathResult(string providerId)
    {
        return new IdentityVerificationResult(
            ProviderId: providerId,
            DocumentStatus: DocumentVerificationStatus.Valid,
            FaceMatch: true,
            Confidence: 0.95m,
            VerifiedAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Creates an invalid document result: Invalid status.
    /// </summary>
    private static IdentityVerificationResult CreateInvalidDocumentResult(string providerId)
    {
        return new IdentityVerificationResult(
            ProviderId: providerId,
            DocumentStatus: DocumentVerificationStatus.Invalid,
            FaceMatch: false,
            Confidence: 0.20m,
            VerifiedAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Creates a face mismatch result: Valid document but face doesn't match.
    /// </summary>
    private static IdentityVerificationResult CreateFaceMismatchResult(string providerId)
    {
        return new IdentityVerificationResult(
            ProviderId: providerId,
            DocumentStatus: DocumentVerificationStatus.Valid,
            FaceMatch: false,
            Confidence: 0.45m,
            VerifiedAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Creates an inconclusive result: Cannot determine document validity.
    /// </summary>
    private static IdentityVerificationResult CreateInconclusiveResult(string providerId)
    {
        return new IdentityVerificationResult(
            ProviderId: providerId,
            DocumentStatus: DocumentVerificationStatus.Inconclusive,
            FaceMatch: false,
            Confidence: 0.50m,
            VerifiedAt: DateTime.UtcNow);
    }
}
