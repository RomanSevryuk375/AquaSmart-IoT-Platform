using Contracts.Results;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Schedules.Commands.DeleteSchedule;

public sealed class DeleteScheduleHandler(
    IScheduleRepository scheduleRepository) : IRequestHandler<DeleteScheduleCommand, Result>
{
    public async Task<Result> Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
    {
        await scheduleRepository.DeleteAsync(request.ScheduleId, cancellationToken);

        return Result.Success();
    }
}
