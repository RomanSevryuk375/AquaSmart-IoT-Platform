using Contracts.Events.RelayEvents;
using Device.Domain.Events.RelayEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class RelayModeChangedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<RelayModeChangedDomainEvent>
{
    public async Task Handle(
        RelayModeChangedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<RelayModeChangedEvent>(notification), cancellationToken);
    }
}
