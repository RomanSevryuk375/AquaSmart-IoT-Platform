using AutoMapper;
using BuildingBlocks.IntegrationEvents.Events.Users;
using IdentityService.Application.Handlers;
using IdentityService.Domain.Events;
using MassTransit;

namespace Identity.Application.UnitTests.Handlers;

public class DomainEventHandlersTests
{
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();
    private readonly IMapper _mapperMock = Substitute.For<IMapper>();

    [Fact]
    public async Task SubscriptionDowngradedEventHandler_PublishesIntegrationEvent()
    {
        // Arrange
        var handler = new SubscriptionDowngradedEventHandler(_publishEndpointMock, _mapperMock);
        var domainEvent = new SubscriptionDowngradedDomainEvent
        {
            UserId = Guid.NewGuid(),
            NewSubscriptionId = Guid.NewGuid()
        };
        var integrationEvent = new SubscriptionDowngradedEvent
        {
            UserId = domainEvent.UserId,
            NewSubscriptionId = domainEvent.NewSubscriptionId
        };
        _mapperMock.Map<SubscriptionDowngradedEvent>(domainEvent).Returns(integrationEvent);

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserCreatedEventHandler_PublishesIntegrationEvent()
    {
        // Arrange
        var handler = new UserCreatedEventHandler(_publishEndpointMock, _mapperMock);
        var domainEvent = new UserCreatedDomainEvent
        {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            PhoneNumber = "+375291112233",
            CreatedAt = DateTime.UtcNow,
            TimeZone = "UTC"
        };
        var integrationEvent = new UserCreatedEvent
        {
            UserId = domainEvent.UserId,
            Email = domainEvent.Email,
            PhoneNumber = domainEvent.PhoneNumber,
            CreatedAt = domainEvent.CreatedAt,
            TimeZone = domainEvent.TimeZone
        };
        _mapperMock.Map<UserCreatedEvent>(domainEvent).Returns(integrationEvent);

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserUpdatedEventHandler_PublishesIntegrationEvent()
    {
        // Arrange
        var handler = new UserUpdatedEventHandler(_publishEndpointMock, _mapperMock);
        var domainEvent = new UserUpdatedDomainEvent
        {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Updated Name",
            PhoneNumber = "+375291112233"
        };
        var integrationEvent = new UserUpdatedEvent
        {
            UserId = domainEvent.UserId,
            Email = domainEvent.Email,
            Name = domainEvent.Name,
            PhoneNumber = domainEvent.PhoneNumber
        };
        _mapperMock.Map<UserUpdatedEvent>(domainEvent).Returns(integrationEvent);

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }
}
