using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Factories;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public class ReminderProcessor(
    IReminderRepository reminderRepository,
    INotificationRepository notificationRepository,
    INotificationSender notificationSender,
    IUnitOfWork unitOfWork) : IReminderProcessor
{
    public async Task CheckAsync(CancellationToken cancellationToken)
    {
        var pendingReminders = await reminderRepository
            .GetPendingRemindersAsync(DateTime.UtcNow, cancellationToken);

        if (pendingReminders is null)
        {
            return;
        }

        foreach (var reminder in pendingReminders)
        {
            if (reminder.LastNotifiedAt?.Date == DateTime.UtcNow.Date)
            {
                continue;
            }

            var (notification, errors) = NotificationEntity.Create(
                reminder.UserId,
                reminder.EcosystemId,
                ReminderImportanceFactory.Evaluate(reminder.NextDueAt),
                $"{reminder.TaskName} should be done at {reminder.NextDueAt:dd.MM.yyyy}");

            if (notification is null)
            {
                continue;
            }

            await notificationSender.ProcessSingleNotificationAsync(notification, cancellationToken);

            reminder.MarkAsNotified();

            await reminderRepository.UpdateAsync(reminder, cancellationToken);
            await notificationRepository.AddAsync(notification, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
