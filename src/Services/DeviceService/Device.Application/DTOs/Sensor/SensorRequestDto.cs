using Contracts.Enums;

namespace Device.Application.DTOs.Sensor;

public record SensorRequestDto
{
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocolEnum ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public SensorTypeEnum Type { get; init; }
    public string Unit { get; init; } = string.Empty;
}
