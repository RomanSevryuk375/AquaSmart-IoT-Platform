using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public sealed record CreateVacationModeRequestDto(
    [property: JsonPropertyName("ecosystemId")] Guid EcosystemId,
    [property: JsonPropertyName("startDate")] DateTime StartDate,
    [property: JsonPropertyName("endDate")] DateTime EndDate,
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("calculatedFeed")] double CalculatedFeed
);
