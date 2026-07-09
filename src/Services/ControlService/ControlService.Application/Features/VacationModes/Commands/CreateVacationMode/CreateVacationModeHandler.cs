using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.VacationModes.Commands.CreateVacationMode;

public sealed class CreateVacationModeHandler(
    IVacationModeRepository vacationModeRepository) : IRequestHandler<CreateVacationModeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateVacationModeCommand request, CancellationToken cancellationToken)
    {
        Result<VacationMode> creationResult = VacationMode.Create(
            vacationModeId: Guid.NewGuid(),
            request.EcosystemId,
            request.StartDate,
            request.EndDate,
            request.IsActive,
            request.CalculatedFeed);
        if (creationResult.IsFailure)
        {
            return Result<Guid>.Failure(creationResult.Error);
        }

        await vacationModeRepository.AddAsync(creationResult.Value, cancellationToken);

        return Result<Guid>.Success(creationResult.Value.Id);
    }
}
