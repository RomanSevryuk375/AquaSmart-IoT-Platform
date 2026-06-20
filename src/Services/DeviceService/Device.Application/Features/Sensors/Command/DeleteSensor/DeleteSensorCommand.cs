using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.DeleteSensor;

internal sealed record DeleteSensorCommand
    : ICommand, ISensorBoundRequest
{
    public Guid SensorId { get; init; }
}
