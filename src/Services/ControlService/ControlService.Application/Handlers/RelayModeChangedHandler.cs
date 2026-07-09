using AutoMapper;
using Contracts.Events.RelayEvents;
using Control.Domain.Events;
using MassTransit;
using MediatR;

namespace Control.Application.Handlers;

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
