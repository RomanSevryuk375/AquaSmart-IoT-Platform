using Contracts.Results;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Ecosystems.Commands.UpdateEcosystem;

public sealed class UpdateEcosystemHandler(
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEcosystemCommand, Result>
{
    public async Task<Result> Handle(
        UpdateEcosystemCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Ecosystem? ecosystem = await ecosystemRepository.GetByIdAsync(request.EcosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound(
                "Ecosystem.NotFound", $"Ecosystem {request.EcosystemId} not found"));
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
