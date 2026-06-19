using Contracts.Events.SensorEvents;
using Device.Domain.Events.SensorEvents;
using MassTransit;

namespace Device.Application.Handlers;

internal class SensorUpdatedHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<SensorUpdatedDomainEvent>
{
    public async Task Handle(
        SensorUpdatedDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new SensorUpdatedEvent
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
