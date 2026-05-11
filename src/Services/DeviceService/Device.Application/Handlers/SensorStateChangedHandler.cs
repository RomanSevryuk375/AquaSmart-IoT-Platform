using Contracts.Events.SensorEvents;
using Device.Domain.DomainEvents.SensorEvents;
using MassTransit;
using MediatR;

namespace Device.Application.Handlers;

public sealed class SensorStateChangedHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<SensorStateChangedDomainEvent>
{
    public async Task Handle(
        SensorStateChangedDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new SensorStateChangedEvent
        {
            SensorId = notification.SensorId,
            State = notification.State
        }, cancellationToken);
    }
}
