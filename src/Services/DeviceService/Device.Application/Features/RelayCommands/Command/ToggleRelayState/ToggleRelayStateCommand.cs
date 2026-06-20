using Contracts.Abstractions;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayState;

internal sealed record ToggleRelayStateCommand : ICommand<bool>
{
    public Guid RelayId { get; init; }
}
