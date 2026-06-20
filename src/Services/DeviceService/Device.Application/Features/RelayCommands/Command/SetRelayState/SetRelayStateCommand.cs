using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.SetRelayState;

internal sealed record SetRelayStateCommand : ICommand, IControllerBoundRequest
{
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public bool TargetState { get; init; }
    public DateTime? ExpireAt { get; init; }
}
