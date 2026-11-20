using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Device;

public record AddSensorRequestDto(
    [property: JsonPropertyName("controllerId")] Guid ControllerId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("connectionProtocol")] int ConnectionProtocol,
    [property: JsonPropertyName("connectionAddress")] string ConnectionAddress,
    [property: JsonPropertyName("type")] int Type
);
