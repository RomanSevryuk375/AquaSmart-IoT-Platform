using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Enums;
using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.AddSensor;

public sealed record AddSensorCommand
    : ICommand<SensorCreatedResponse>, IControllerBoundRequest
{
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public SensorType Type { get; init; }
}
