using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.VacationModes.Commands.UpdateVacationMode;

public sealed class UpdateVacationModeHandler(
    IVacationModeRepository vacationModeRepository) : IRequestHandler<UpdateVacationModeCommand, Result>
{
    public async Task<Result> Handle(UpdateVacationModeCommand request, CancellationToken cancellationToken)
    {
        VacationMode? vacationMode = await vacationModeRepository.GetByIdAsync(
            request.VacationModeId, cancellationToken);

        Result timingResult = vacationMode!.SetTiming(request.StartDate, request.EndDate);
        if (timingResult.IsFailure)
        {
            return Result.Failure(timingResult.Error);
        }

        Result feedResult = vacationMode.SetFeedSize(request.CalculatedFeed);
        if (feedResult.IsFailure)
        {
            return Result.Failure(feedResult.Error);
        }

        return Result.Success();
    }
}
