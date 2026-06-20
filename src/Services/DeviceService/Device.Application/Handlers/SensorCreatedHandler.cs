using Contracts.Events.SensorEvents;
using Device.Domain.DomainEvents.SensorEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class SensorCreatedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<SensorCreatedDomainEvent>
{
    public async Task Handle(
        SensorCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<SensorCreatedEvent>(notification), cancellationToken);
    }
}
