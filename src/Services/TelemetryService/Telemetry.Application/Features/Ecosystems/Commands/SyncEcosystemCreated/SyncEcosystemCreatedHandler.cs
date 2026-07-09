using Contracts.Results;
using MediatR;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;

public sealed class SyncEcosystemCreatedHandler(IEcosystemRepository ecosystemRepository)
    : IRequestHandler<SyncEcosystemCreatedCommand, Result>
{
    public async Task<Result> Handle(SyncEcosystemCreatedCommand request, CancellationToken cancellationToken)
    {
        Ecosystem? existingEcosystem = await ecosystemRepository.GetByIdAsync(
            request.EcosystemId, cancellationToken);
        if (existingEcosystem is not null)
        {
            return Result.Success();
        }

        Result<Ecosystem> result = Ecosystem.Create(
            request.EcosystemId,
            request.ControllerId,
            request.UserId);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await ecosystemRepository.AddAsync(result.Value, cancellationToken);

        return Result.Success();
    }
}
