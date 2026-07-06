using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Ecosystems.Commands.UpdateEcosystem;

public sealed class UpdateEcosystemHandler(IEcosystemRepository ecosystemRepository)
    : IRequestHandler<UpdateEcosystemCommand, Result>
{
    public async Task<Result> Handle(
        UpdateEcosystemCommand request,
        CancellationToken cancellationToken)
    {
        Ecosystem? ecosystem = await ecosystemRepository.GetByIdAsync(
            request.EcosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem {request.EcosystemId} not found"));
        }

        Result nameResult = ecosystem.SetName(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        Result volumeResult = ecosystem.SetVolume(request.Volume);
        if (volumeResult.IsFailure)
        {
            return Result.Failure(volumeResult.Error);
        }

        return Result.Success();
    }
}
