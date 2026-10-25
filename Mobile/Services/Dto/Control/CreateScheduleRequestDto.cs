using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public sealed record CreateScheduleRequestDto(
    [property: JsonPropertyName("ecosystemId")] Guid EcosystemId,
    [property: JsonPropertyName("relayId")] Guid RelayId,
    [property: JsonPropertyName("cronExpression")] string CronExpression,
    [property: JsonPropertyName("durationMin")] double DurationMin,
    [property: JsonPropertyName("isFadeMode")] bool IsFadeMode,
    [property: JsonPropertyName("isEnabled")] bool IsEnabled
);
