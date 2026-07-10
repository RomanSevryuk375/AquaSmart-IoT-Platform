using AutoMapper;
using Contracts.Events.TelemetryEvents;
using MassTransit;
using Telemetry.Application.DTOs;
using Telemetry.Application.Handlers;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Events;

namespace Telemetry.Application.UnitTests.Handlers;

public class RawTelemetryAddedEventHandlerTests
{
    private readonly ITelemetryNotifier _realtimeNotifierMock;
    private readonly IPublishEndpoint _publishEndpointMock;
    private readonly IMapper _mapperMock;
    private readonly RawTelemetryAddedEventHandler _handler;

    public RawTelemetryAddedEventHandlerTests()
    {
        _realtimeNotifierMock = Substitute.For<ITelemetryNotifier>();
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _mapperMock = Substitute.For<IMapper>();

        _handler = new RawTelemetryAddedEventHandler(
            _realtimeNotifierMock,
            _publishEndpointMock,
            _mapperMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Suppressed")]
    public async Task Handle_WhenCalledWithDomainEvent_PublishesEventAndNotifiesSignalR()
    {
        // Arrange
        var domainEvent = new RawTelemetryAddedDomainEvent
        {
            SensorId = Guid.NewGuid(),
            EcosystemId = Guid.NewGuid(),
            Value = 45.2,
            RecordedAt = DateTime.UtcNow,
            ExternalMessageId = "msg_789"
        };

        var expectedPointDto = new TelemetryRawChartPointDto
        {
            SensorId = domainEvent.SensorId,
            Value = domainEvent.Value,
            RecordedAt = domainEvent.RecordedAt
        };

        var expectedIntegrationEvent = new TelemetryReceivedEvent
        {
            SensorId = domainEvent.SensorId,
            Value = domainEvent.Value,
            RecordedAt = domainEvent.RecordedAt
        };

        _mapperMock.Map<TelemetryRawChartPointDto>(domainEvent).Returns(expectedPointDto);
        _mapperMock.Map<TelemetryReceivedEvent>(domainEvent).Returns(expectedIntegrationEvent);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await _realtimeNotifierMock.Received(1).TelemetryRawReceived(
            domainEvent.EcosystemId.ToString(),
            Arg.Is<TelemetryRawChartPointDto>(p => p.SensorId == expectedPointDto.SensorId &&
                                                 Math.Abs(p.Value - expectedPointDto.Value) < 0.001 &&
                                                 p.RecordedAt == expectedPointDto.RecordedAt));

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<TelemetryReceivedEvent>(e => e.SensorId == expectedIntegrationEvent.SensorId &&
                                               Math.Abs(e.Value - expectedIntegrationEvent.Value) < 0.001 &&
                                               e.RecordedAt == expectedIntegrationEvent.RecordedAt),
            Arg.Any<CancellationToken>());
    }
}
