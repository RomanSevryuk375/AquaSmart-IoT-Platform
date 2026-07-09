using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Application.Features.Sensors.Commands.SyncSensorState;

public sealed record SyncSensorStateCommand : ICommand
{
    public Guid SensorId { get; init; }
    public SensorState State { get; init; }
}
