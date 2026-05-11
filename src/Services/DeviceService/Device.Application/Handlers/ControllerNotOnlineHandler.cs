using Contracts.Events.ControllerEvents;
using Device.Domain.DomainEvents.ControllerEvents;
using MassTransit;
using MediatR;

namespace Device.Application.Handlers;

public sealed class ControllerNotOnlineHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<ControllerNotOnlineDomainEvent>
{
    public async Task Handle(
        ControllerNotOnlineDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new ControllerNotOnlineEvent
        {
            ControllerId = notification.ControllerId,
            UserId = notification.UserId,
            LastSeenAt = notification.LastSeenAt,
        }, cancellationToken);
    }
}
