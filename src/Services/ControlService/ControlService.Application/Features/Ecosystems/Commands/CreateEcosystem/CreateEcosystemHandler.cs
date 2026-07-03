using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MassTransit;
using MediatR;

namespace Control.Application.Features.Ecosystems.Commands.CreateEcosystem;

public sealed class CreateEcosystemHandler(
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateEcosystemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateEcosystemCommand request,
        CancellationToken cancellationToken)
    {
        Result<Ecosystem> createResult = Ecosystem.Create(
            NewId.NextGuid(), request.UserId,
            request.Type, request.Name, request.Volume, request.ControllerId);
        if (createResult.IsFailure)
        {
            return Result<Guid>.Failure(createResult.Error);
        }

        Guid result = await ecosystemRepository.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result);
    }
}
