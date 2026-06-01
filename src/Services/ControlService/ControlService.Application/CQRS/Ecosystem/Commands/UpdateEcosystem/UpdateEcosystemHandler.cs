using Contracts.Results;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Commands.UpdateEcosystem;

public sealed class UpdateEcosystemHandler(
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEcosystemCommand, Result>
{
    public async Task<Result> Handle(
        UpdateEcosystemCommand request, 
        CancellationToken cancellationToken)
    {
        var ecosystem = await ecosystemRepository.GetByIdAsync(request.EcosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound(
                "Ecosystem.NotFound", $"Ecosystem {request.EcosystemId} not found"));
        }

        var nameResult = ecosystem.SetName(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        var volumeResult = ecosystem.SetVolume(request.Volume);
        if (volumeResult.IsFailure)
        {
            return Result.Failure(volumeResult.Error);
        }

        await ecosystemRepository.UpdateAsync(ecosystem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
