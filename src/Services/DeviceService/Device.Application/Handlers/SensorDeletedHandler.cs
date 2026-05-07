using Contracts.Events.SensorEvents;
using Device.Domain.DomainEvents.SensorEvents;
using MassTransit;
using MediatR;

namespace Device.Application.Handlers;

public sealed class SensorDeletedHandler(IPublishEndpoint publishEndpoint) 
    : INotificationHandler<SensorDeletedDomainEvent>
{
    public async Task Handle(
        SensorDeletedDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new SensorDeletedEvent
        {
            SensorId = notification.SensorId,
        }, cancellationToken);
    }
}
