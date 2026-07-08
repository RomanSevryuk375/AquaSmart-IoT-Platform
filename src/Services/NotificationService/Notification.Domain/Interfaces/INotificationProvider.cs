using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface INotificationProvider
{
    public bool IsEnabled(User user);
    public Task<(bool Success, string Error)> SendAsync(
        User user, string message, CancellationToken cancellationToken);
}
