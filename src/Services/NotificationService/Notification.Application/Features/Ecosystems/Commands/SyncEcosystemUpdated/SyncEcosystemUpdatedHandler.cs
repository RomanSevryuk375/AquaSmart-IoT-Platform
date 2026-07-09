using AutoMapper;
using Contracts.Results;
using MediatR;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Ecosystems.Commands.SyncEcosystemUpdated;

internal class SyncEcosystemUpdatedHandler(
    IEcosystemRepository ecosystemRepository,
    IMapper mapper, ISender sender)
    : IRequestHandler<SyncEcosystemUpdatedCommand, Result>
{
    public async Task<Result> Handle(SyncEcosystemUpdatedCommand request, CancellationToken cancellationToken)
    {
        Ecosystem? existingEcosystem = await ecosystemRepository.GetByIdAsync(
            request.EcosystemId, cancellationToken);

        if (existingEcosystem is null)
        {
            SyncEcosystemCreatedCommand command = mapper.Map<SyncEcosystemCreatedCommand>(request);
            Result createResult = await sender.Send(command, cancellationToken);
            if (createResult.IsFailure)
            {
                return Result.Failure(createResult.Error);
            }
        }

        existingEcosystem!.SetName(request.Name);

        return Result.Success();
    }
}
