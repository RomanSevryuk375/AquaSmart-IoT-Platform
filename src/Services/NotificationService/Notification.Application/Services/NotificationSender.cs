using Contracts.Results;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public class NotificationSender(
    IEnumerable<INotificationProvider> providers,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : INotificationSender
{
    public async Task ProcessSingleNotificationAsync(
        Domain.Entities.Notification notification,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository
            .GetByIdAsync(notification.UserId, cancellationToken);

        bool overallSuccess = false;
        var errors = new List<string>();

        if (user == null || !user.IsNotifyEnabled)
        {
            return;
        }

        foreach (INotificationProvider provider in providers)
        {
            if (provider.IsEnabled(user))
            {
                Result result = await provider
                    .SendAsync(user, notification.Message.Value, cancellationToken);

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

        if (overallSuccess)
        {
            notification.MarkAsPublished();
        }
        else
        {
            notification.MarkAsFailure(string.Join(" | ", errors));
        }


        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
