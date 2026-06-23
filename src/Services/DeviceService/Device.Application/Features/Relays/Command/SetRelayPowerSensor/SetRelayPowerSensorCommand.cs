using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Relays.Command.SetRelayPowerSensor;

public sealed record SetRelayPowerSensorCommand
    : ICommand, IRelayBoundRequest
{
    public Guid RelayId { get; init; }
    public Guid PowerSensorId { get; init; }
}
