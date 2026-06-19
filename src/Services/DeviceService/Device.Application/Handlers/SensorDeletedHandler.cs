using Contracts.Events.SensorEvents;
using Device.Domain.Events.SensorEvents;
using MassTransit;

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
