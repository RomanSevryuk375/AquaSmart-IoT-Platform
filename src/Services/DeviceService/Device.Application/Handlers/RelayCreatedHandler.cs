using Contracts.Events.RelayEvents;
using Device.Domain.Events.RelayEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class RelayCreatedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<RelayCreatedDomainEvent>
{
    public async Task Handle(
        RelayCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<RelayCreatedEvent>(notification), cancellationToken);
    }
}
