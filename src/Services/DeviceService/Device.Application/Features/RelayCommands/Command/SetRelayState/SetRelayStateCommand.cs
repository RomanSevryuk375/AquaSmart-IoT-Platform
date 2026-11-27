using BuildingBlocks.Domain.Abstractions;

namespace Device.Application.Features.RelayCommands.Command.SetRelayState;

public sealed record SetRelayStateCommand : ICommand
{
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public bool TargetState { get; init; }
    public DateTime? ExpireAt { get; init; }
}
