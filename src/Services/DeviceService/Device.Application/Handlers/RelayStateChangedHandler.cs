using Contracts.Events.RelayEvents;
using Device.Domain.Events.RelayEvents;
using MassTransit;
using MediatR;

namespace Device.Application.Handlers;

public sealed class RelayStateChangedHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<RelayStateChangedDomainEvent>
{
    public async Task Handle(
        RelayStateChangedDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new ChangeRelayStateEvent
        {
            RelayId = notification.RelayId,
            Action = notification.Action,
            ExpireAt = notification.ExpireAt,
        }, cancellationToken);
    }
}
