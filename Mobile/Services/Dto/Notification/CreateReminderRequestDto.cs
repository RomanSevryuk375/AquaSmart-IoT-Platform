using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Notification;

public sealed record CreateReminderRequestDto(
    [property: JsonPropertyName("ecosystemId")] Guid EcosystemId,
    [property: JsonPropertyName("taskName")] string TaskName,
    [property: JsonPropertyName("intervalDays")] int IntervalDays
);
