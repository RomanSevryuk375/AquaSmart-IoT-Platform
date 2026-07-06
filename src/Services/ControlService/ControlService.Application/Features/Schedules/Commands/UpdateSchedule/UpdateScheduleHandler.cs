using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Schedules.Commands.UpdateSchedule;

public sealed class UpdateScheduleHandler(
    IScheduleRepository scheduleRepository,
    ICronValidator cronValidator) : IRequestHandler<UpdateScheduleCommand, Result>
{
    public async Task<Result> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        Schedule? schedule = await scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);

        Result updateResult = schedule!.Update(
            request.CronExpression, cronValidator, request.DurationMin,
            request.IsFadeMode, request.IsEnabled);

        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        return Result.Success();
    }
}
