using Contracts.Abstractions;

namespace Notification.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand : ICommand
{
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
}
