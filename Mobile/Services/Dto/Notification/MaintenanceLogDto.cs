using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Notification;

public sealed record MaintenanceLogDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; init; }

    [JsonPropertyName("ecosystemId")]
    public Guid EcosystemId { get; init; }

    [JsonPropertyName("actionDate")]
    public DateTime ActionDate { get; init; }

    [JsonPropertyName("metrics")]
    public Dictionary<string, double> Metrics { get; init; } = [];

    [JsonPropertyName("notes")]
    public string Notes { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }
}
