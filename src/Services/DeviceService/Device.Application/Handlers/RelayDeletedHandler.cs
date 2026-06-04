using Contracts.Events.RelayEvents;
using Device.Domain.Events.RelayEvents;
using MassTransit;
using MediatR;

namespace Device.Application.Handlers;

public sealed class RelayDeletedHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<RelayDeletedDomainEvent>
{
    public async Task Handle(
        RelayDeletedDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new RelayDeletedEvent
        {
            RelayId = notification.RelayId,
        }, cancellationToken);
    }
}
