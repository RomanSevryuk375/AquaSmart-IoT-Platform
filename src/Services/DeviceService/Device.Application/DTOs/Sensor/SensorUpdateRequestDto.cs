namespace Device.Application.DTOs.Sensor;

public sealed record SensorUpdateRequestDto
{
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public Guid ControllerId { get; init; }
    public SensorType Type { get; init; }
    public string Unit { get; init; } = string.Empty;
}
