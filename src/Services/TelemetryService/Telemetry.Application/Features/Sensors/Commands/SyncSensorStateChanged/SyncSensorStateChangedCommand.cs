using Contracts.Abstractions;
using Contracts.Enums;

namespace Telemetry.Application.Features.Sensors.Commands.SyncSensorStateChanged;

public sealed record SyncSensorStateChangedCommand : ICommand
{
    public Guid SensorId { get; init; }
    public SensorState State { get; init; }
}
