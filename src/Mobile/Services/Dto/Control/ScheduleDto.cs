using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public sealed record ScheduleDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("ecosystemId")]
    public Guid EcosystemId { get; init; }

    [JsonPropertyName("relayId")]
    public Guid RelayId { get; init; }

    [JsonPropertyName("cronExpression")]
    public string CronExpression { get; init; } = string.Empty;

    [JsonPropertyName("durationMin")]
    public double DurationMin { get; init; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; init; }

    [JsonPropertyName("isFadeMode")]
    public bool IsFadeMode { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    // Friendly display property
    [JsonIgnore]
    public string FriendlyDescription => $"Включение по расписанию ({DurationMin:F0} мин.)";
}
