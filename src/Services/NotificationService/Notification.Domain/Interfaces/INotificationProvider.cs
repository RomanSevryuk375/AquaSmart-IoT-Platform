using Contracts.Results;
using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface INotificationProvider
{
    public bool IsEnabled(User user);
    public Task<Result> SendAsync(
        User user,
        string message,
        CancellationToken cancellationToken = default);
}
