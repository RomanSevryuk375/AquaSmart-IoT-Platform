using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Relays.Command.UpdateRelay;

public sealed record UpdateRelayCommand
    : ICommand, IRelayBoundRequest, IControllerBoundRequest
{
    public Guid RelayId { get; init; }
    public Guid ControllerId { get; init; }
    public ConnectionProtocol ConnectionProtocol { get; init; }
    public string ConnectionAddress { get; init; } = string.Empty;
    public RelayPurpose Purpose { get; init; }
    public bool IsNormallyOpen { get; init; }
}
