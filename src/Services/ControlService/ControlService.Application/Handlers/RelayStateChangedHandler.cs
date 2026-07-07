using AutoMapper;
using Contracts.Events.RelayEvents;
using Control.Domain.Events;
using MassTransit;
using MediatR;

namespace Control.Application.Handlers;

public sealed class RelayStateChangedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<RelayStateChangedDomainEvent>
{
    public async Task Handle(
        RelayStateChangedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<ChangeRelayStateEvent>(notification), cancellationToken);
    }
}
