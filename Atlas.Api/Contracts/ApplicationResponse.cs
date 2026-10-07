namespace Atlas.Api.Contracts;

using System.Text.Json.Serialization;

/// <summary>
/// Response model returned after successfully submitting an application.
/// </summary>
public record ApplicationResponse
{
    [JsonPropertyName("applicationId")]
    public Guid ApplicationId { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}
