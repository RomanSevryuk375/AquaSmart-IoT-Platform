using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using Control.Domain.Interfaces;
using MassTransit;
using MediatR;

namespace Control.Application.Features.Ecosystem.Commands.DeleteEcosystem;

public sealed class DeleteEcosystemHandler(
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IRequestHandler<DeleteEcosystemCommand, Result>
{
    public async Task<Result> Handle(
        DeleteEcosystemCommand request,
        CancellationToken cancellationToken)
    {
        await ecosystemRepository.DeleteAsync(request.EcosystemId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            new EcosystemDeletedEvent { EcosystemId = request.EcosystemId },
            cancellationToken);

        return Result.Success();
    }
}
