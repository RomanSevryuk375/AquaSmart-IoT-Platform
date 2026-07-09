using Contracts.Abstractions;

namespace Telemetry.Application.Features.Sensors.Commands.SyncSensorNameChanged;

public sealed record SyncSensorNameChangedCommand : ICommand
{
    public Guid SensorId { get; init; }
    public string Name { get; init; } = string.Empty;
}
