using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Ecosystems.Commands.DeleteEcosystem;

public sealed class DeleteEcosystemHandler(
    IEcosystemRepository ecosystemRepository) : IRequestHandler<DeleteEcosystemCommand, Result>
{
    public async Task<Result> Handle(
        DeleteEcosystemCommand request,
        CancellationToken cancellationToken)
    {
        Ecosystem? ecosystem = await ecosystemRepository.GetByIdAsync(
            request.EcosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem {request.EcosystemId} not found"));
        }

        ecosystem.MarkAsDeleted();

        await ecosystemRepository.DeleteAsync(request.EcosystemId, cancellationToken);

        return Result.Success();
    }
}
