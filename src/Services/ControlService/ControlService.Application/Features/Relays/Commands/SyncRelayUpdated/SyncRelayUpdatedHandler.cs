using AutoMapper;
using Contracts.Results;
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

        DateTime expireAt = DateTime.UtcNow.AddMinutes(5);

        existingRelay!.SetPurpose(request.Purpose);
        existingRelay.SetMode(request.IsManual);
        existingRelay.SetState(request.IsActive, expireAt);
        Result nameResult = existingRelay.SetName(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        return Result.Success();
    }
}
