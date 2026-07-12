using Contracts.Constants;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.InternalEvents;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;

public sealed class ProcessUnpublishedNoticesHandler(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IPublishEndpoint publishEndpoint) : IRequestHandler<ProcessUnpublishedNoticesCommand, Result>
{
    public async Task<Result> Handle(ProcessUnpublishedNoticesCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Entities.Notification> notifications = await notificationRepository
            .GetUnpublishedNotificationsAsync(cancellationToken);
        if (notifications.Count == 0)
        {
            return Result.Success();
        }

        var userIds = notifications.Select(n => n.UserId).Distinct().ToList();
        List<User> users = await userRepository.GetAllUsersByIdAsync(userIds, cancellationToken);
        var usersDict = users.ToDictionary(u => u.Id);

        foreach (Domain.Entities.Notification notification in notifications)
        {
            if (!usersDict.TryGetValue(notification.UserId, out User? user) || !user.IsNotifyEnabled)
            {
                notification.MarkAsFailure(ErrorMessages.NotificationProvider.UserDisabledOrNotFound);
                continue;
            }
            if (!user.TgEnable && !user.EmailEnable)
            {
                notification.MarkAsFailure(ErrorMessages.NotificationProvider.NoActiveChannels);
                continue;
            }

            if (user.TgEnable)
            {
                await publishEndpoint.Publish(new SendTelegramCommand
                {
                    NotificationId = notification.Id,
                    ChatId = user.TelegramChatId!.Value,
                    Message = notification.Message.Value
                }, cancellationToken);
            }

            if (user.EmailEnable)
            {
                await publishEndpoint.Publish(new SendEmailCommand
                {
                    NotificationId = notification.Id,
                    Email = user.Email.Value,
                    Message = notification.Message.Value
                }, cancellationToken);
            }

            notification.MarkAsPublished();
        }

        return Result.Success();
    }
}
