using Contracts.Events.RelayEvents;
using Device.Domain.Events.RelayEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class RelayCreatedHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<RelayCreatedDomainEvent>
{
    public async Task Handle(
        RelayCreatedDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new RelayCreatedEvent
        {
            RelayId = notification.RelayId,
            ControllerId = notification.ControllerId,
            PowerSensorId = notification.PowerSensorId,
            Name = notification.Name,
            Purpose = notification.Purpose,
            IsManual = notification.IsManual,
            IsActive = notification.IsActive,
            CreatedAt = notification.CreatedAt,
        }, cancellationToken);
    }
}
