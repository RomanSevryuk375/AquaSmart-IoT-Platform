using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.UpdateSensor;

public sealed record UpdateSensorCommand
    : ICommand, ISensorBoundRequest, IControllerBoundRequest
{
    public Guid SensorId { get; init; }
    public Guid ControllerId { get; init; }
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
