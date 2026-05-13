using Contracts.Events.RelayEvents;
using Device.Domain.Events.RelayEvents;
using MassTransit;
using MediatR;

namespace Device.Application.Handlers;

public sealed class RelayUpdatedHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<RelayUpdatedDomainEvent>
{
    public async Task Handle(
        RelayUpdatedDomainEvent notification, 
        CancellationToken cancellationToken)
    {
         await publishEndpoint.Publish(new RelayUpdatedEvent
         {
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
