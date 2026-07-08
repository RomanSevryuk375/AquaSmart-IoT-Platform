using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Control.Domain.Events;
using MassTransit;
using MediatR;

namespace Control.Application.Handlers;

public sealed class EcosystemUpdatedHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<EcosystemUpdatedDomainEvent>
{
    public async Task Handle(
        EcosystemUpdatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            mapper.Map<EcosystemUpdatedEvent>(notification), cancellationToken);
    }
}
