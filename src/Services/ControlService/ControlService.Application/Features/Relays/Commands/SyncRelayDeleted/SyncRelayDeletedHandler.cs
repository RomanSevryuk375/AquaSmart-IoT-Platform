using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Relays.Commands.SyncRelayDeleted;

public sealed class SyncRelayDeletedHandler(IRelayRepository relayRepository)
    : IRequestHandler<SyncRelayDeletedCommand, Result>
{
    public async Task<Result> Handle(SyncRelayDeletedCommand request, CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(request.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result.Success();
        }

        await relayRepository.DeleteAsync(request.RelayId, cancellationToken);

        return Result.Success();
    }
}
