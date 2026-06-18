using Contracts.Enums;

namespace Device.Application.DTOs.Sensor;

public sealed record SensorRequestDto
{
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public SensorType Type { get; init; }
    public string Unit { get; init; } = string.Empty;
}
