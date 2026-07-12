// Ignore Spelling: Tg

using Contracts.Results;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.Interfaces;

public interface INotificationProvider
{
    public Task<Result> SendAsync(
        NotificationRecipient recipient,
        string message,
        CancellationToken cancellationToken = default);
}
