using Contracts.Abstractions;

namespace Device.Application.Features.Relays.Command.AddRelay;

internal sealed record AddRelayCommand : ICommand<RelayCreatedResponse>
{
    public Guid ControllerId { get; init; }
    public Guid? PowerSensorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public bool IsNormalyOpen { get; init; }
    public RelayPurpose Purpose { get; init; }
    public bool IsActive { get; init; }
    public bool IsManual { get; init; }
}
