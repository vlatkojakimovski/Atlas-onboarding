namespace Atlas.Infrastructure.ExternalServices.SanctionsScreening;

using Atlas.Domain.Enums;
using Atlas.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Mock implementation of sanctions screening service for development and testing.
/// Simulates various screening scenarios based on configuration.
/// Implements requirements 4.1-4.6.
/// </summary>
public class MockSanctionsScreeningService : ISanctionsScreeningService
{
    private readonly ILogger<MockSanctionsScreeningService> _logger;
    private readonly IConfiguration _configuration;

    public MockSanctionsScreeningService(
        ILogger<MockSanctionsScreeningService> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Simulates sanctions screening with configurable test scenarios.
    /// </summary>
    public async Task<SanctionsScreeningResult> ScreenAsync(
        SanctionsScreeningRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Starting mock sanctions screening for: {FirstName} {LastName}, DOB: {DateOfBirth}, Nationality: {Nationality}",
            request.FirstName, request.LastName, request.DateOfBirth.ToShortDateString(), request.Nationality);

        // Get mock scenario from configuration or use default (clear)
        var mockScenario = _configuration["MockServices:SanctionsScreening:Scenario"] ?? "Clear";
        var delayMs = _configuration.GetValue<int>("MockServices:SanctionsScreening:DelayMs", 500);

        _logger.LogDebug(
            "Using mock scenario: {Scenario} with delay: {DelayMs}ms",
            mockScenario, delayMs);

        // Simulate network delay
        await Task.Delay(delayMs, cancellationToken);

        // Generate mock case ID
        var caseId = $"WC-{Guid.NewGuid():N}";

        SanctionsScreeningResult result = mockScenario.ToLowerInvariant() switch
        {
            "clear" => CreateClearResult(caseId),
            "possiblematch" or "sanctions" => CreateSanctionsMatchResult(caseId),
            "pep" => CreatePepMatchResult(caseId),
            "timeout" => throw new TaskCanceledException("Mock sanctions screening service timeout"),
            _ => CreateClearResult(caseId) // Default to clear
        };

        _logger.LogInformation(
            "Mock sanctions screening completed. CaseId: {CaseId}, Status: {Status}, MatchCount: {MatchCount}",
            result.CaseId, result.Status, result.Matches.Count);

        return result;
    }

    /// <summary>
    /// Creates a clear result: No sanctions or PEP matches.
    /// </summary>
    private static SanctionsScreeningResult CreateClearResult(string caseId)
    {
        return new SanctionsScreeningResult(
            CaseId: caseId,
            Status: ScreeningStatus.Clear,
            Matches: Array.Empty<SanctionMatch>(),
            ScreenedAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Creates a sanctions match result: PossibleMatch with sanctions hit.
    /// </summary>
    private static SanctionsScreeningResult CreateSanctionsMatchResult(string caseId)
    {
        var matches = new List<SanctionMatch>
        {
            new SanctionMatch(
                Type: MatchType.Sanctions,
                Score: 0.75m,
                Subject: "OFAC Specially Designated Nationals List - Possible Match")
        };

        return new SanctionsScreeningResult(
            CaseId: caseId,
            Status: ScreeningStatus.PossibleMatch,
            Matches: matches.AsReadOnly(),
            ScreenedAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Creates a PEP match result: PossibleMatch with PEP hit.
    /// </summary>
    private static SanctionsScreeningResult CreatePepMatchResult(string caseId)
    {
        var matches = new List<SanctionMatch>
        {
            new SanctionMatch(
                Type: MatchType.PEP,
                Score: 0.80m,
                Subject: "Politically Exposed Person - Government Official")
        };

        return new SanctionsScreeningResult(
            CaseId: caseId,
            Status: ScreeningStatus.PossibleMatch,
            Matches: matches.AsReadOnly(),
            ScreenedAt: DateTime.UtcNow);
    }
}
