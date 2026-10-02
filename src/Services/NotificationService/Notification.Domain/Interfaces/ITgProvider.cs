// Ignore Spelling: Tg

using BuildingBlocks.Domain.Results;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.Interfaces;

public interface ITgProvider
{
    public Task<Result> SendAsync(
        NotificationRecipient recipient,
        string message,
        object? inlineKeyboard = null,
        CancellationToken cancellationToken = default);
}
