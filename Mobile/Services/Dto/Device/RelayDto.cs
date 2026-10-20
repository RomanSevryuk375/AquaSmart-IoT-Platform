using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Device;

public record RelayDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("controllerId")] Guid ControllerId,
    [property: JsonPropertyName("powerSensorId")] Guid? PowerSensorId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("connectionProtocol")] int ConnectionProtocol,
    [property: JsonPropertyName("connectionAddress")] string ConnectionAddress,
    [property: JsonPropertyName("isNormallyOpen")] bool IsNormallyOpen,
    [property: JsonPropertyName("purpose")] int Purpose, // 0: Filter, 1: Boiler, 2: Light, 3: Pump, 4: Heating
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("isManual")] bool IsManual,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);
