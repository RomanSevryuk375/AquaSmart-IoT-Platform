using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Notification;

public sealed record ReminderDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; init; }

    [JsonPropertyName("ecosystemId")]
    public Guid EcosystemId { get; init; }

    [JsonPropertyName("taskName")]
    public string TaskName { get; init; } = string.Empty;

    [JsonPropertyName("intervalDays")]
    public int IntervalDays { get; init; }

    [JsonPropertyName("lastDoneAt")]
    public DateTime? LastDoneAt { get; init; }

    [JsonPropertyName("lastNotifiedAt")]
    public DateTime? LastNotifiedAt { get; init; }

    [JsonPropertyName("nextDueAt")]
    public DateTime NextDueAt { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    [JsonIgnore]
    public bool IsOverdue => DateTime.UtcNow > NextDueAt;

    [JsonIgnore]
    public string StatusText => IsOverdue
        ? $"Срок истёк ({NextDueAt:dd.MM.yyyy})"
        : $"Срок: {NextDueAt:dd.MM.yyyy}";

    [JsonIgnore]
    public string StatusColor => IsOverdue ? "#E53935" : "#4152B0";
}
