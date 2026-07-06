using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Relays.Commands.SyncRelayMode;

public sealed class SyncRelayModeHandler(IRelayRepository relayRepository)
    : IRequestHandler<SyncRelayModeCommand, Result>
{
    public async Task<Result> Handle(SyncRelayModeCommand request, CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound<Relay>(
                $"Relay {request.RelayId} not found. "));
        }

        existingRelay.SetMode(request.IsManual);

        return Result.Success();
    }
}
