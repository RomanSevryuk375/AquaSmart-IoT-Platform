using AutoMapper;
using BuildingBlocks.Domain.Results;
using Control.Application.Features.Relays.Commands.SyncRelayCreated;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Relays.Commands.SyncRelayUpdated;

public sealed class SyncRelayUpdatedHandler(
    IRelayRepository relayRepository,
    ISender sender,
    IMapper mapper) : IRequestHandler<SyncRelayUpdatedCommand, Result>
{
    public async Task<Result> Handle(SyncRelayUpdatedCommand request, CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository
            .GetByIdAsync(request.RelayId, cancellationToken);

        if (existingRelay is null)
        {
            SyncRelayCreatedCommand createCommand = mapper.Map<SyncRelayCreatedCommand>(request);
            Result creationResult = await sender.Send(createCommand, cancellationToken);
            if (!creationResult.IsSuccess)
            {
                return creationResult;
            }

            return Result.Success();
        }

        existingRelay!.SetPurpose(request.Purpose);
        existingRelay.SyncMode(request.IsManual);
        existingRelay.SyncState(request.IsActive);
        Result nameResult = existingRelay.SetName(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        return Result.Success();
    }
}
