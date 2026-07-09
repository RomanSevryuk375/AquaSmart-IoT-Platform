using Contracts.Results;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.VacationModes.Commands.DeleteVacationMode;

public sealed class DeleteVacationModeHandler(
    IVacationModeRepository vacationModeRepository) : IRequestHandler<DeleteVacationModeCommand, Result>
{
    public async Task<Result> Handle(DeleteVacationModeCommand request, CancellationToken cancellationToken)
    {
        await vacationModeRepository.DeleteAsync(request.VacationModeId, cancellationToken);

        return Result.Success();
    }
}
