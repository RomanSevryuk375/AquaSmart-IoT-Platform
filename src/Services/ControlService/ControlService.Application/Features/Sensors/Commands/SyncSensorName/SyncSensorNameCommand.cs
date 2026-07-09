using Contracts.Abstractions;

namespace Control.Application.Features.Sensors.Commands.SyncSensorName;

public sealed record SyncSensorNameCommand : ICommand
{
    public Guid SensorId { get; init; }
    public string Name { get; init; } = string.Empty;
}
