using Contracts.Events.SensorEvents;
using MassTransit;
using MediatR;
using Telemetry.Domain.Events;

namespace Telemetry.Application.Handlers;

public sealed class SensorNoDataHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<SensorNoDataDomainEvent>
{
    public async Task Handle(
        SensorNoDataDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new SensorNoDataEvent
        {
            SensorId = notification.SensorId,
            State = notification.State,
            LastSeenAt = notification.LastSeenAt,
        }, cancellationToken);
    }
}
