using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Notification;

public sealed record CreateMaintenanceLogRequestDto(
    [property: JsonPropertyName("ecosystemId")] Guid EcosystemId,
    [property: JsonPropertyName("actionDate")] DateTime ActionDate,
    [property: JsonPropertyName("metrics")] Dictionary<string, double>? Metrics,
    [property: JsonPropertyName("notes")] string Notes
);
