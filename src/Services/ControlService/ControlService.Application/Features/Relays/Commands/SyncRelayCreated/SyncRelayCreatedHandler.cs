using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Relays.Commands.SyncRelayCreated;

public sealed class SyncRelayCreatedHandler(
    IRelayRepository relayRepository,
    IEcosystemRepository ecosystemRepository) : IRequestHandler<SyncRelayCreatedCommand, Result>
{
    public async Task<Result> Handle(SyncRelayCreatedCommand request, CancellationToken cancellationToken)
    {
        if (await relayRepository.ExistsAsync(request.RelayId, cancellationToken))
        {
            return Result.Success();
        }

        Ecosystem? ecosystem = await ecosystemRepository.GetByControllerIdAsync(
            request.ControllerId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem with controller {request.ControllerId} not found. "));
        }

        Result<Relay> result = Relay.Create(
            request.RelayId, ecosystem.Id, request.ControllerId, request.PowerSensorId,
            request.Name, request.Purpose, request.IsManual, request.IsActive, request.CreatedAt);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await relayRepository.AddAsync(result.Value, cancellationToken);

        return Result.Success();
    }
}
