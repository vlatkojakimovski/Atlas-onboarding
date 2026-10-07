namespace Atlas.Api.Contracts;

using System.Text.Json.Serialization;

/// <summary>
/// Response model containing full application details.
/// </summary>
public record ApplicationDetailsResponse
{
    [JsonPropertyName("applicationId")]
    public Guid ApplicationId { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; init; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; init; } = string.Empty;

    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; init; }

    [JsonPropertyName("market")]
    public string Market { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; init; }

    [JsonPropertyName("accountId")]
    public Guid? AccountId { get; init; }

    [JsonPropertyName("rejectionReasons")]
    public List<string>? RejectionReasons { get; init; }
}
