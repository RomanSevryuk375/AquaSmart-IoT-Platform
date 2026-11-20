using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Device;

public record SensorDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("controllerId")] Guid ControllerId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("connectionProtocol")] int ConnectionProtocol,
    [property: JsonPropertyName("connectionAddress")] string ConnectionAddress,
    [property: JsonPropertyName("type")] int Type, // 1: Temperature, 2: Pressure, 3: Humidity, 4: Voltage
    [property: JsonPropertyName("state")] int State,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("lastValue")] double LastValue,
    [property: JsonPropertyName("isDataDelayed")] bool IsDataDelayed,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);
