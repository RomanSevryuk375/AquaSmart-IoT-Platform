using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.SetSensorState;

internal sealed record SetSensorStateCommand
    : ICommand, ISensorBoundRequest
{
    public Guid SensorId { get; init; }
    public SensorState SensorState { get; init; }
}
