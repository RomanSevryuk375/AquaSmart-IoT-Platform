using BuildingBlocks.IntegrationEvents.Events.Relays;
using Device.Domain.Events.RelayEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class RelayStateChangedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<RelayStateChangedDomainEvent>
{
    public async Task Handle(
        RelayStateChangedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<RelayStateSyncedEvent>(notification), cancellationToken);
    }
}
