using AutoMapper;
using Contracts.Events.UserEvents;
using IdentityService.Domain.Events;
using MassTransit;
using MediatR;

namespace IdentityService.Application.Handlers;

public sealed class UserCreatedEventHandler(
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<UserCreatedDomainEvent>
{
    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        UserCreatedEvent integrationEvent = mapper.Map<UserCreatedEvent>(notification);

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
