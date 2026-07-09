using Contracts.Abstractions;

namespace Telemetry.Application.Features.Sensors.Commands.SyncSensorDeleted;

public sealed record SyncSensorDeletedCommand : ICommand
{
    public Guid SensorId { get; init; }
}
