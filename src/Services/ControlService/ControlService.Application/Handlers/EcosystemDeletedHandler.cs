using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Control.Domain.Events;
using MassTransit;
using MediatR;

namespace Control.Application.Handlers;

public sealed class EcosystemDeletedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<EcosystemDeletedDomainEvent>
{
    public async Task Handle(
        EcosystemDeletedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<EcosystemDeletedEvent>(notification), cancellationToken);
    }
}
