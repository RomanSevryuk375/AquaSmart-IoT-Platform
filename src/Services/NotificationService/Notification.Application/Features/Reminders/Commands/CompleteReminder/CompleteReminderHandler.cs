using Contracts.Results;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Reminders.Commands.CompleteReminder;

public sealed class CompleteReminderHandler(
    IReminderRepository reminderRepository,
    IUserContext userContext)
    : IRequestHandler<CompleteReminderCommand, Result>
{
    public async Task<Result> Handle(CompleteReminderCommand request, CancellationToken cancellationToken)
    {
        Reminder? reminder = await reminderRepository.GetByIdAsync(request.ReminderId, cancellationToken);
        if (reminder is null)
        {
            return Result.Failure(Error.NotFound("Reminder.NotFound", $"Reminder {request.ReminderId} not found."));
        }

        if (reminder.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict("Access.Denied", "You are not the owner of this reminder."));
        }

        reminder.CompleteTask();

        return Result.Success();
    }
}
