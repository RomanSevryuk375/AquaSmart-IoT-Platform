using Contracts.Results;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;

public sealed class SyncEcosystemCreatedHandler(
    IEcosystemRepository ecosystemRepository,
    IUserRepository userRepository) : IRequestHandler<SyncEcosystemCreatedCommand, Result>
{
    public async Task<Result> Handle(SyncEcosystemCreatedCommand request, CancellationToken cancellationToken)
    {
        Ecosystem? existingEcosystem = await ecosystemRepository.GetByIdAsync(
            request.EcosystemId, cancellationToken);
        if (existingEcosystem is not null)
        {
            return Result.Success();
        }

        bool existingUser = await userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!existingUser)
        {
            return Result.Failure(Error.NotFound<User>(
                $"User {request.UserId} not found. "));
        }

        Result<Ecosystem> ecosystemResult = Ecosystem.Create(
            request.EcosystemId, request.UserId, request.Name);
        if (ecosystemResult.IsFailure)
        {
            return Result.Failure(ecosystemResult.Error);
        }

        await ecosystemRepository.AddAsync(ecosystemResult.Value, cancellationToken);

        return Result.Success();
    }
}
