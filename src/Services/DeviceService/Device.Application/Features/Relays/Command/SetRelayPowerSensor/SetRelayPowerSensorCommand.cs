using Contracts.Abstractions;

namespace Device.Application.Features.Relays.Command.SetRelayPowerSensor;

internal sealed record SetRelayPowerSensorCommand : ICommand
{
    public Guid RelayId { get; init; }
    public Guid PowerSensorId { get; init; }
}
