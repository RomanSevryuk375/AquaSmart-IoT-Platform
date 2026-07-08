using Contracts.Results;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;

public sealed class ProcessUnpublishedNoticesHandler(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IEnumerable<INotificationProvider> providers) : IRequestHandler<ProcessUnpublishedNoticesCommand, Result>
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
                notification.MarkAsFailure("User disabled notifications or not found.");
                continue;
            }

            bool overallSuccess = false;
            var errors = new List<string>();

#pragma warning disable S3267 
            foreach (INotificationProvider provider in providers)
            {
                if (provider.IsEnabled(user))
                {
                    Result result = await provider.SendAsync(user, notification.Message.Value, cancellationToken);

                    if (result.IsSuccess)
                    {
                        overallSuccess = true;
                    }
                    else
                    {
                        errors.Add(result.Error.Message);
                    }
                }
            }
#pragma warning restore S3267 

            if (overallSuccess)
            {
                notification.MarkAsPublished();
            }
            else
            {
                string failureReason = errors.Count > 0
                    ? string.Join(" | ", errors)
                    : "No enabled notification providers found for user.";

                notification.MarkAsFailure(failureReason);
            }
        }

        return Result.Success();
    }
}
