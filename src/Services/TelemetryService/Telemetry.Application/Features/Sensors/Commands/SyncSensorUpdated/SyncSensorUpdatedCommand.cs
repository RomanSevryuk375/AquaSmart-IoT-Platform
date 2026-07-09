using Contracts.Abstractions;
using Contracts.Enums;

namespace Telemetry.Application.Features.Sensors.Commands.SyncSensorUpdated;

public sealed record SyncSensorUpdatedCommand : ICommand
{
    public Guid SensorId { get; init; }
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public SensorType Type { get; init; }
    public SensorState State { get; init; }
    public string Unit { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
