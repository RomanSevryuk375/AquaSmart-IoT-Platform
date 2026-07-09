using Contracts.Abstractions;

namespace Notification.Application.Features.Alerts.Commands.SendSubscriptionAlert;

public sealed record SendSubscriptionAlertCommand : ICommand
{
    public Guid UserId { get; init; }
    public Guid NewSubscriptionId { get; init; }
    public DateTime OccurredOn { get; init; }
}
