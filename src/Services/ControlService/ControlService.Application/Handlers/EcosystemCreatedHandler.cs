using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Control.Domain.Events;
using MassTransit;
using MediatR;

namespace Control.Application.Handlers;

public sealed class EcosystemCreatedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<EcosystemCreatedDomainEvent>
{
    public async Task Handle(
        EcosystemCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<EcosystemCreatedEvent>(notification), cancellationToken);
    }
}
