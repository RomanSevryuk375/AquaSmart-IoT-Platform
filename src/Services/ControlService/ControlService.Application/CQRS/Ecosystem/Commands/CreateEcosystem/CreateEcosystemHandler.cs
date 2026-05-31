using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Commands.CreateEcosystem;

public sealed class CreateEcosystemHandler(
    IEcosystemRepository ecosystemRepository, 
    IUnitOfWork unitOfWork) : IRequestHandler<CreateEcosystemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateEcosystemCommand request, 
        CancellationToken cancellationToken)
    {
        var createResult = EcosystemEntity.Create(
            request.UserId,
            request.Type,
            request.Name,
            request.Volume,
            request.ControllerId);
        if (createResult.IsFailure)
        {
            return Result<Guid>.Failure(createResult.Error);
        }

        var result = await ecosystemRepository.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result);
    }
}
