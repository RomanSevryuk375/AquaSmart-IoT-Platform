using Contracts.Events.SensorEvents;
using Device.Domain.DomainEvents.SensorEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class SensorCreatedHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<SensorCreatedDomainEvent>
{
    public async Task Handle(
        SensorCreatedDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new SensorCreatedEvent
        {
            SensorId = notification.SensorId,
            ControllerId = notification.ControllerId,
            Name = notification.Name,
            Type = notification.Type,
            State = notification.State,
            Unit = notification.Unit,
            CreatedAt = notification.CreatedAt,
        }, cancellationToken);
    }
}
