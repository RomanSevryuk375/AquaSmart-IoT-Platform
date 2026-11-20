using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Device;

public record AddRelayRequestDto(
    [property: JsonPropertyName("controllerId")] Guid ControllerId,
    [property: JsonPropertyName("powerSensorId")] Guid? PowerSensorId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("connectionProtocol")] int ConnectionProtocol,
    [property: JsonPropertyName("connectionAddress")] string ConnectionAddress,
    [property: JsonPropertyName("isNormallyOpen")] bool IsNormallyOpen,
    [property: JsonPropertyName("purpose")] int Purpose,
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("isManual")] bool IsManual
);
