using AutoMapper;
using BuildingBlocks.Domain.Results;
using MediatR;
using Notification.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Ecosystems.Commands.SyncEcosystemUpdated;

internal sealed class SyncEcosystemUpdatedHandler(
    IEcosystemRepository ecosystemRepository,
    IMapper mapper, ISender sender)
    : IRequestHandler<SyncEcosystemUpdatedCommand, Result>
{
    public async Task<Result> Handle(SyncEcosystemUpdatedCommand request, CancellationToken cancellationToken)
    {
        Ecosystem? existingEcosystem = await ecosystemRepository.GetByIdAsync(request.EcosystemId, cancellationToken);
        if (existingEcosystem is null)
        {
            SyncEcosystemCreatedCommand command = mapper.Map<SyncEcosystemCreatedCommand>(request);
            return await sender.Send(command, cancellationToken);
        }

        existingEcosystem.SetName(request.Name);

        return Result.Success();
    }
}
