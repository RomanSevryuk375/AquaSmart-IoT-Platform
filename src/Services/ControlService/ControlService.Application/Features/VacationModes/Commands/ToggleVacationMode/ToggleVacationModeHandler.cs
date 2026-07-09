using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.VacationModes.Commands.ToggleVacationMode;

public sealed class ToggleVacationModeHandler(
    IVacationModeRepository vacationModeRepository) : IRequestHandler<ToggleVacationModeCommand, Result>
{
    public async Task<Result> Handle(ToggleVacationModeCommand request, CancellationToken cancellationToken)
    {
        VacationMode? vacationMode = await vacationModeRepository.GetByIdAsync(
            request.VacationModeId, cancellationToken);

        vacationMode!.SetActive(!vacationMode.IsActive);

        return Result.Success();
    }
}
