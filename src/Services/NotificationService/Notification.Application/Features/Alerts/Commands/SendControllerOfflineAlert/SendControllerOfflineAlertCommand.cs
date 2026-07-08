using Contracts.Abstractions;

namespace Notification.Application.Features.Alerts.Commands.SendControllerOfflineAlert;

public sealed record SendControllerOfflineAlertCommand : ICommand
{
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
    public DateTime LastSeenAt { get; init; }
}
