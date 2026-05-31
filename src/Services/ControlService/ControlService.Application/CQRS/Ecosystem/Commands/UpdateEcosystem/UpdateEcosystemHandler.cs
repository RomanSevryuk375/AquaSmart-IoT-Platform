using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Commands.UpdateEcosystem;

public sealed class UpdateEcosystemHandler(
    IEcosystemRepository ecosystemRepository,
    ISecureService secureService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEcosystemCommand, Result>
{
    public async Task<Result> Handle(
        UpdateEcosystemCommand request, 
        CancellationToken cancellationToken)
    {
        var ownership = await secureService.EnsureUserOwnsEcosystemAsync(
            request.EcosystemId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        var ecosystem = ownership.Value;
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
