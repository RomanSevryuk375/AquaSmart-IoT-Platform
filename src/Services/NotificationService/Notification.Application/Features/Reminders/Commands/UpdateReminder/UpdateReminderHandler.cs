using Contracts.Results;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Reminders.Commands.UpdateReminder;

public sealed class UpdateReminderHandler(IReminderRepository reminderRepository)
    : IRequestHandler<UpdateReminderCommand, Result>
{
    public async Task<Result> Handle(UpdateReminderCommand request, CancellationToken cancellationToken)
    {
        Reminder? reminder = await reminderRepository.GetByIdAsync(request.ReminderId, cancellationToken);

        Result updateResult = reminder!.UpdateSchedule(request.TaskName, request.IntervalDays);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        return Result.Success();
    }
}
