using Contracts.Abstractions;

namespace Device.Application.Features.Relays.Command.UpdateRelay;

internal sealed record UpdateRelayCommand : ICommand
{
    public Guid RelayId { get; init; }
    public Guid ControllerId { get; init; }
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public RelayPurpose Purpose { get; init; }
    public bool IsNormalyOpen { get; init; }
}
