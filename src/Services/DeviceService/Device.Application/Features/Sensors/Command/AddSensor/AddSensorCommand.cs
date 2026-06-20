using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.AddSensor;

internal class AddSensorCommand
    : ICommand<SensorCreatedResponse>, IControllerBoundRequest
{
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public SensorType Type { get; init; }
    public string Unit { get; init; } = string.Empty;
}
