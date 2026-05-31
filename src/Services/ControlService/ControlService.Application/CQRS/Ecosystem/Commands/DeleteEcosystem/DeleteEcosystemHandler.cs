using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Interfaces;
using MassTransit;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Commands.DeleteEcosystem;

public sealed class DeleteEcosystemHandler(
    ISecureService secureService, 
    IEcosystemRepository ecosystemRepository, 
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IRequestHandler<DeleteEcosystemCommand, Result>
{
    public async Task<Result> Handle(
        DeleteEcosystemCommand request, 
        CancellationToken cancellationToken)
    {
        var ownership = await secureService.EnsureUserOwnsEcosystemAsync(
            request.EcosystemId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        await ecosystemRepository.DeleteAsync(request.EcosystemId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            new EcosystemDeletedEvent { EcosystemId = request.EcosystemId },
            cancellationToken);

        return Result.Success();
    }
}
