using Contracts.Abstractions;

namespace Device.Application.Features.Sensors.Command.DeleteSensor;

internal sealed record DeleteSensorCommand : ICommand
{
    public Guid SensorId { get; init; }
}
