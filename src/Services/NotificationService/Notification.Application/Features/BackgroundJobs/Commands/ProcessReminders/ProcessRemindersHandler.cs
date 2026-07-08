using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Factories;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.BackgroundJobs.Commands.ProcessReminders;

public sealed class ProcessRemindersHandler(
    IReminderRepository reminderRepository,
    INotificationRepository notificationRepository,
    IUserRepository userRepository) : IRequestHandler<ProcessRemindersCommand, Result>
{
    public async Task<Result> Handle(ProcessRemindersCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Reminder>? pendingReminders = await reminderRepository.GetPendingRemindersAsync(
            now: DateTime.UtcNow, cancellationToken);
        if (pendingReminders is null)
        {
            return Result.Success();
        }

        var userIds = pendingReminders.Select(x => x.UserId).Distinct().ToList();
        var users = (await userRepository.GetAllUsersByIdAsync(userIds, cancellationToken)).ToDictionary(u => u.Id);
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
                DateTime lastNotifiedLocal = TimeZoneInfo.ConvertTimeFromUtc(
                    reminder.LastNotifiedAt.Value, tzInfo);
                if (lastNotifiedLocal.Date == userLocalTime.Date)
                {
                    continue;
                }
            }

            if (InDaytime(userLocalTime))
            {
                await NotifyUserAsync(reminder, cancellationToken);
            }
        }

        return Result.Success();
    }

    private static bool InDaytime(DateTime userLocalTime) => userLocalTime.Hour >= 9 && userLocalTime.Hour <= 21;

    private async Task NotifyUserAsync(Reminder reminder, CancellationToken cancellationToken)
    {
        Result<Domain.Entities.Notification>? notificationResult = Domain.Entities.Notification.Create(
            notificationId: NewId.NextGuid(), reminder.UserId, reminder.EcosystemId,
            level: ReminderImportanceFactory.Evaluate(reminder.NextDueAt),
            rawMessage: $"{reminder.TaskName} should be done at {reminder.NextDueAt:dd.MM.yyyy}");
        if (notificationResult.IsFailure)
        {
            return;
        }

        reminder.MarkAsNotified();

        await notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
    }
}
