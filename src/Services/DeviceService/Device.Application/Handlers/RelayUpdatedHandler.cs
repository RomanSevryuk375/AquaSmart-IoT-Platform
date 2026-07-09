using Contracts.Events.RelayEvents;
using Device.Domain.Events.RelayEvents;
using MassTransit;

namespace Device.Application.Handlers;

public sealed class RelayUpdatedHandler(
    IPublishEndpoint publishEndpoint, IMapper mapper)
    : INotificationHandler<RelayUpdatedDomainEvent>
{
    public async Task Handle(
        RelayUpdatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<RelayUpdatedEvent>(notification), cancellationToken);
    }
}
