using Contracts.Events.SensorEvents;
using Device.Domain.Events.SensorEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class SensorStateChangedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<SensorStateChangedDomainEvent>
{
    public async Task Handle(
        SensorStateChangedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<SensorStateChangedEvent>(notification), cancellationToken);
    }
}
