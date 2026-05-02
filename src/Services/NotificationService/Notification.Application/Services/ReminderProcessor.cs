using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Factories;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public class ReminderProcessor(
    IReminderRepository reminderRepository,
    INotificationRepository notificationRepository,
    INotificationSender notificationSender,
    IUserRepository userRepository,
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

        var userIds = pendingReminders.Select(x => x.UserId).Distinct().ToList();
        var users = (await userRepository.GetAllUsersByIdAsync(userIds, cancellationToken))
            .ToDictionary(u => u.Id);

        foreach (var reminder in pendingReminders)
        {
            if (!users.TryGetValue(reminder.UserId, out var user))
            {
                continue;
            }

            TimeZoneInfo tzInfo;
            try
            {
                tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone ?? "UTC");
            }
            catch (TimeZoneNotFoundException)
            {
                tzInfo = TimeZoneInfo.Utc;
            }

            var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzInfo);

            if (reminder.LastNotifiedAt.HasValue)
            {
                var lastNotifiedLocal = TimeZoneInfo.ConvertTimeFromUtc(reminder.LastNotifiedAt.Value, tzInfo);
                if (lastNotifiedLocal.Date == userLocalTime.Date)
                {
                    continue; 
                }
            }

            if (userLocalTime.Hour >= 9 && userLocalTime.Hour <= 21)
            {
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
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
