using Contracts.Events.SensorEvents;
using Device.Domain.Events.SensorEvents;
using MassTransit;

namespace Device.Application.Handlers;

internal class SensorUpdatedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<SensorUpdatedDomainEvent>
{
    public async Task Handle(
        SensorUpdatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<SensorUpdatedEvent>(notification), cancellationToken);
    }
}
