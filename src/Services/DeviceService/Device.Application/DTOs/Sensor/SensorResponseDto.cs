using Contracts.Enums;

namespace Device.Application.DTOs.Sensor;

public record SensorResponseDto
{
    public Guid Id { get; init; }
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocolEnum ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public SensorTypeEnum Type { get; init; }
    public SensorStateEnum State { get; init; }
    public string Unit { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
