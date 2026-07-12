using Contracts.Constants;
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
            return Result.Failure(Error.NotFound<Reminder>(
                string.Format(ErrorMessages.Reminder.NotFoundFormat, request.ReminderId)));
        }

        if (reminder.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(ErrorCodes.Security.AccessDenied,
                ErrorMessages.Security.YouAreNotOwnerOfReminder));
        }

        reminder.CompleteTask();

        return Result.Success();
    }
}
