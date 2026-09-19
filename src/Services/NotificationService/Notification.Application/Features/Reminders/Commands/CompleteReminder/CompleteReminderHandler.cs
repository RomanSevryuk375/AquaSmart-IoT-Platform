using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using MediatR;
using Notification.Application.Constants;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace Notification.Application.Features.Reminders.Commands.CompleteReminder;

public sealed class CompleteReminderHandler(
    IReminderRepository reminderRepository,
    IFusionCache cache)
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

        if (reminder.UserId != request.UserId)
        {
            return Result.Failure(Error.Forbidden(
                ErrorCodes.Security.AccessDenied,
                ErrorMessages.Security.YouAreNotOwnerOfReminder));
        }

        reminder.CompleteTask();

        await cache.RemoveAsync(CacheKeys.Reminder(reminder.UserId, reminder.Id), token: cancellationToken);

        return Result.Success();
    }
}
