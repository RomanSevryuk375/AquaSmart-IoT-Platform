using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public sealed record VacationModeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("ecosystemId")]
    public Guid EcosystemId { get; init; }

    [JsonPropertyName("dateRange")]
    public string DateRange { get; init; } = string.Empty;

    [JsonPropertyName("calculatedFeed")]
    public double CalculatedFeed { get; init; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }
}
