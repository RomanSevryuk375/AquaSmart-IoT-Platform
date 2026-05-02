using Contracts.Enums;

namespace Device.Application.DTOs.Configurations;

public sealed record SensorConfigDto
{
    public Guid SensorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocolEnum ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public SensorTypeEnum Type { get; init; }
    public string Unit { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
