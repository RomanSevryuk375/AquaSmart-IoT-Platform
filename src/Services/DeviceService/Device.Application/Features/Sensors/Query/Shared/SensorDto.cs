// Ignore Spelling: Dto

namespace Device.Application.Features.Sensors.Query.Shared;

public sealed record SensorDto
{
    public Guid Id { get; init; }
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public SensorType Type { get; init; }
    public SensorState State { get; init; }
    public string Unit { get; init; } = string.Empty;
    public double LastValue { get; init; }
    public bool IsDataDelayed { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
