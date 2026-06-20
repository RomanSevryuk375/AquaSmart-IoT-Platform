using Contracts.Abstractions;

namespace Device.Application.Features.Sensors.Command.UpdateSensor;

internal sealed record UpdateSensorCommand : ICommand
{
    public Guid SensorId { get; init; }
    public Guid ControllerId { get; init; }
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;}
