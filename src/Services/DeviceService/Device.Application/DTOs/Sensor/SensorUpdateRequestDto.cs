using Contracts.Enums;

namespace Device.Application.DTOs.Sensor;

public record SensorUpdateRequestDto
{
    public ConnectionProtocolEnum ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public Guid ControllerId { get; init; }
    public SensorTypeEnum Type { get; init; }
    public string Unit { get; init; } = string.Empty;
}
