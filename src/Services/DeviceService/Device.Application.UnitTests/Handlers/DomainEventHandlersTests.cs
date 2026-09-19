using AutoMapper;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.IntegrationEvents.Events.Controllers;
using BuildingBlocks.IntegrationEvents.Events.Relays;
using BuildingBlocks.IntegrationEvents.Events.Sensors;
using Device.Application.Handlers;
using Device.Domain.Events.ControllerEvents;
using Device.Domain.Events.RelayEvents;
using Device.Domain.Events.SensorEvents;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Device.Application.UnitTests.Handlers;

public class DomainEventHandlersTests
{
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();
    private readonly IMapper _mapperMock = Substitute.For<IMapper>();
    private readonly IFusionCache _cacheMock = Substitute.For<IFusionCache>();

    [Fact]
    public async Task ControllerCacheInvalidationHandler_RemovesCacheKey()
    {
        var handler = new ControllerCacheInvalidationHandler(_cacheMock);
        var domainEvent = new ControllerNotOnlineDomainEvent
        {
            UserId = Guid.NewGuid(),
            ControllerId = Guid.NewGuid(),
            LastSeenAt = DateTime.UtcNow
        };

        await handler.Handle(domainEvent, CancellationToken.None);

        await _cacheMock.Received(1).RemoveAsync(Arg.Any<string>(), token: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ControllerNotOnlineHandler_PublishesIntegrationEvent()
    {
        var handler = new ControllerNotOnlineHandler(_publishEndpointMock, _mapperMock);
        var domainEvent = new ControllerNotOnlineDomainEvent
        {
            UserId = Guid.NewGuid(),
            ControllerId = Guid.NewGuid(),
            LastSeenAt = DateTime.UtcNow
        };
        var integrationEvent = new ControllerNotOnlineEvent();
        _mapperMock.Map<ControllerNotOnlineEvent>(domainEvent).Returns(integrationEvent);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RelayCreatedHandler_PublishesIntegrationEvent()
    {
        var handler = new RelayCreatedHandler(_publishEndpointMock, _mapperMock);
        Relay relay = new RelayBuilder().Build();
        var domainEvent = new RelayCreatedDomainEvent
        {
            RelayId = relay.Id,
            ControllerId = relay.ControllerId,
            Name = relay.Name.Value,
            Purpose = relay.Purpose,
            IsManual = relay.IsManual,
            IsActive = relay.IsActive
        };
        var integrationEvent = new RelayCreatedEvent();
        _mapperMock.Map<RelayCreatedEvent>(domainEvent).Returns(integrationEvent);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RelayDeletedHandler_PublishesIntegrationEvent()
    {
        var handler = new RelayDeletedHandler(_publishEndpointMock);
        var domainEvent = new RelayDeletedDomainEvent { UserId = Guid.NewGuid(), RelayId = Guid.NewGuid() };

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(Arg.Is<RelayDeletedEvent>(e => e.RelayId == domainEvent.RelayId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RelayModeChangedHandler_PublishesIntegrationEvent()
    {
        var handler = new RelayModeChangedHandler(_publishEndpointMock, _mapperMock);
        var domainEvent = new RelayModeChangedDomainEvent { UserId = Guid.NewGuid(), RelayId = Guid.NewGuid(), IsManual = true };
        var integrationEvent = new RelayModeChangedEvent();
        _mapperMock.Map<RelayModeChangedEvent>(domainEvent).Returns(integrationEvent);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RelayStateChangedHandler_PublishesIntegrationEvent()
    {
        var handler = new RelayStateChangedHandler(_publishEndpointMock, _mapperMock);
        var domainEvent = new RelayStateChangedDomainEvent
        {
            ControllerId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            TargetState = true
        };
        var integrationEvent = new ChangeRelayStateEvent();
        _mapperMock.Map<ChangeRelayStateEvent>(domainEvent).Returns(integrationEvent);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RelayUpdatedHandler_PublishesIntegrationEvent()
    {
        var handler = new RelayUpdatedHandler(_publishEndpointMock, _mapperMock);
        Relay relay = new RelayBuilder().Build();
        var domainEvent = new RelayUpdatedDomainEvent
        {
            RelayId = relay.Id,
            UserId = relay.UserId,
            ControllerId = relay.ControllerId,
            Name = relay.Name.Value,
            Purpose = relay.Purpose,
            IsManual = relay.IsManual,
            IsActive = relay.IsActive
        };
        var integrationEvent = new RelayUpdatedEvent();
        _mapperMock.Map<RelayUpdatedEvent>(domainEvent).Returns(integrationEvent);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetRelayPowerSensorHandler_PublishesIntegrationEvent()
    {
        var handler = new Device.Application.Handlers.SetRelayPowerSensorHandler(_publishEndpointMock, _mapperMock);
        var domainEvent = new SetRelayPowerSensorDomainEvent
        {
            UserId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            PowerSensorId = Guid.NewGuid()
        };
        var integrationEvent = new SetRelayPowerSensorEvent();
        _mapperMock.Map<SetRelayPowerSensorEvent>(domainEvent).Returns(integrationEvent);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RelayCacheInvalidationHandler_HandlesAllEvents()
    {
        var handler = new RelayCacheInvalidationHandler(_cacheMock);
        Relay relay = new RelayBuilder().Build();

        await handler.Handle(new RelayUpdatedDomainEvent
        {
            RelayId = relay.Id,
            UserId = relay.UserId,
            ControllerId = relay.ControllerId,
            Name = relay.Name.Value,
            Purpose = relay.Purpose,
            IsManual = relay.IsManual,
            IsActive = relay.IsActive
        }, CancellationToken.None);

        await handler.Handle(new SetRelayPowerSensorDomainEvent
        {
            UserId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            PowerSensorId = Guid.NewGuid()
        }, CancellationToken.None);

        await handler.Handle(new RelayStateChangedDomainEvent
        {
            ControllerId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            TargetState = true
        }, CancellationToken.None);

        await handler.Handle(new RelayModeChangedDomainEvent
        {
            UserId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            IsManual = true
        }, CancellationToken.None);

        await handler.Handle(new RelayDeletedDomainEvent
        {
            UserId = Guid.NewGuid(),
            RelayId = Guid.NewGuid()
        }, CancellationToken.None);

        await _cacheMock.Received(5).RemoveAsync(Arg.Any<string>(), token: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SensorCacheInvalidationHandler_HandlesAllEvents()
    {
        var handler = new SensorCacheInvalidationHandler(_cacheMock);
        Sensor sensor = new SensorBuilder().Build();

        await handler.Handle(new SensorUpdatedDomainEvent
        {
            SensorId = sensor.Id,
            UserId = sensor.UserId,
            ControllerId = sensor.ControllerId,
            Name = sensor.Name.Value,
            Type = sensor.Type,
            State = sensor.State,
            Unit = sensor.Unit
        }, CancellationToken.None);

        await handler.Handle(new SensorStateChangedDomainEvent
        {
            SensorId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            State = SensorState.Faulty
        }, CancellationToken.None);

        await handler.Handle(new SensorDeletedDomainEvent
        {
            SensorId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        }, CancellationToken.None);

        await _cacheMock.Received(3).RemoveAsync(Arg.Any<string>(), token: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SensorCreatedHandler_PublishesIntegrationEvent()
    {
        var handler = new SensorCreatedHandler(_publishEndpointMock, _mapperMock);
        Sensor sensor = new SensorBuilder().Build();
        var domainEvent = new SensorCreatedDomainEvent
        {
            SensorId = sensor.Id,
            UserId = sensor.UserId,
            ControllerId = sensor.ControllerId,
            Name = sensor.Name.Value,
            Type = sensor.Type,
            State = sensor.State,
            Unit = sensor.Unit
        };
        var integrationEvent = new SensorCreatedEvent();
        _mapperMock.Map<SensorCreatedEvent>(domainEvent).Returns(integrationEvent);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SensorDeletedHandler_PublishesIntegrationEvent()
    {
        var handler = new SensorDeletedHandler(_publishEndpointMock);
        var domainEvent = new SensorDeletedDomainEvent
        {
            SensorId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(Arg.Is<SensorDeletedEvent>(e => e.SensorId == domainEvent.SensorId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SensorStateChangedHandler_PublishesIntegrationEvent()
    {
        var handler = new SensorStateChangedHandler(_publishEndpointMock, _mapperMock);
        var domainEvent = new SensorStateChangedDomainEvent
        {
            SensorId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            State = SensorState.Faulty
        };
        var integrationEvent = new SensorStateChangedEvent();
        _mapperMock.Map<SensorStateChangedEvent>(domainEvent).Returns(integrationEvent);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SensorUpdatedHandler_PublishesIntegrationEvent()
    {
        var handler = new SensorUpdatedHandler(_publishEndpointMock, _mapperMock);
        Sensor sensor = new SensorBuilder().Build();
        var domainEvent = new SensorUpdatedDomainEvent
        {
            SensorId = sensor.Id,
            UserId = sensor.UserId,
            ControllerId = sensor.ControllerId,
            Name = sensor.Name.Value,
            Type = sensor.Type,
            State = sensor.State,
            Unit = sensor.Unit
        };
        var integrationEvent = new SensorUpdatedEvent();
        _mapperMock.Map<SensorUpdatedEvent>(domainEvent).Returns(integrationEvent);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _publishEndpointMock.Received(1).Publish(integrationEvent, Arg.Any<CancellationToken>());
    }
}
