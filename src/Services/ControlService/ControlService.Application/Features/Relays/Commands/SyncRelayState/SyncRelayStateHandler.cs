using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Relays.Commands.SyncRelayState;

public sealed class SyncRelayStateHandler(IRelayRepository relayRepository)
    : IRequestHandler<SyncRelayStateCommand, Result>
{
    public async Task<Result> Handle(SyncRelayStateCommand request, CancellationToken cancellationToken)
    {
        DateTime expireAt = DateTime.UtcNow.AddMinutes(5);

        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound<Relay>(
                $"Relay {request.RelayId} not found. "));
        }

        existingRelay.SetState(request.TargetState, expireAt);

        return Result.Success();
    }
}
