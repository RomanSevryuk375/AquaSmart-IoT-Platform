using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Device;

public record ControllerRegisteredResponse(
    [property: JsonPropertyName("controllerId")] Guid ControllerId,
    [property: JsonPropertyName("deviceToken")] string DeviceToken
);

