using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Application.Features.Sensors.Commands.SyncSensorUpdated;

public sealed record SyncSensorUpdatedCommand : ICommand
{
    public Guid SensorId { get; init; }
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public SensorType Type { get; init; }
    public SensorState State { get; init; }
    public DateTime CreatedAt { get; init; }
}
