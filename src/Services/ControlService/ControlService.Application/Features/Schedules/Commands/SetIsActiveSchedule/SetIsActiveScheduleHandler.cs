using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Schedules.Commands.SetIsActiveSchedule;

public sealed class SetIsActiveScheduleHandler(IScheduleRepository scheduleRepository)
    : IRequestHandler<SetIsActiveScheduleCommand, Result>
{
    public async Task<Result> Handle(SetIsActiveScheduleCommand request, CancellationToken cancellationToken)
    {
        Schedule? schedule = await scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);

        schedule!.SetIsActive(request.IsActive);

        return Result.Success();
    }
}
