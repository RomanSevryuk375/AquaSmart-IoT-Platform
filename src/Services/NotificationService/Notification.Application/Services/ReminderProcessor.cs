using Contracts.Results;
using MassTransit;
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
        IReadOnlyList<Reminder>? pendingReminders = await reminderRepository
            .GetPendingRemindersAsync(DateTime.UtcNow, cancellationToken);

        if (pendingReminders is null)
        {
            return;
        }

        var userIds = pendingReminders.Select(x => x.UserId).Distinct().ToList();
        var users = (await userRepository.GetAllUsersByIdAsync(userIds, cancellationToken))
            .ToDictionary(u => u.Id);

        foreach (Reminder reminder in pendingReminders)
        {
            if (!users.TryGetValue(reminder.UserId, out User? user))
            {
                continue;
            }

            TimeZoneInfo tzInfo;
            try
            {
                tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone.Value ?? "UTC");
            }
            catch (TimeZoneNotFoundException)
            {
                tzInfo = TimeZoneInfo.Utc;
            }

            DateTime userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzInfo);

            if (reminder.LastNotifiedAt.HasValue)
            {
                DateTime lastNotifiedLocal = TimeZoneInfo.ConvertTimeFromUtc(reminder.LastNotifiedAt.Value, tzInfo);
                if (lastNotifiedLocal.Date == userLocalTime.Date)
                {
                    continue;
                }
            }

            if (userLocalTime.Hour >= 9 && userLocalTime.Hour <= 21)
            {
                Result<Domain.Entities.Notification>? notificationResult = Domain.Entities.Notification.Create(
                    NewId.NextGuid(),
                    reminder.UserId,
                    reminder.EcosystemId,
                    ReminderImportanceFactory.Evaluate(reminder.NextDueAt),
                    $"{reminder.TaskName} should be done at {reminder.NextDueAt:dd.MM.yyyy}");

                if (notificationResult is null)
                {
                    continue;
                }

                await notificationSender.ProcessSingleNotificationAsync(notificationResult.Value, cancellationToken);

                reminder.MarkAsNotified();

                await notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
