using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Relays.Command.DeleteRelay;

public sealed record DeleteRelayCommand
    : ICommand, IRelayBoundRequest
{
    public Guid RelayId { get; init; }
}
