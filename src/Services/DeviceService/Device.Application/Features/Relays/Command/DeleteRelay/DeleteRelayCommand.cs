using Contracts.Abstractions;

namespace Device.Application.Features.Relays.Command.DeleteRelay;

internal sealed record DeleteRelayCommand : ICommand
{
    public Guid RelayId { get; init; }
}
