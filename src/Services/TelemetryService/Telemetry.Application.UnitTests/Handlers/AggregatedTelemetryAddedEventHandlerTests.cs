using AutoMapper;
using Telemetry.Application.DTOs;
using Telemetry.Application.Handlers;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Events;

namespace Telemetry.Application.UnitTests.Handlers;

public class AggregatedTelemetryAddedEventHandlerTests
{
    private readonly ITelemetryNotifier _realtimeNotifierMock;
    private readonly IMapper _mapperMock;
    private readonly AggregatedTelemetryAddedEventHandler _handler;

    public AggregatedTelemetryAddedEventHandlerTests()
    {
        _realtimeNotifierMock = Substitute.For<ITelemetryNotifier>();
        _mapperMock = Substitute.For<IMapper>();

        _handler = new AggregatedTelemetryAddedEventHandler(
            _realtimeNotifierMock,
            _mapperMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Suppressed")]
    public async Task Handle_WhenCalledWithDomainEvent_NotifiesSignalR()
    {
        // Arrange
        var domainEvent = new AggregatedTelemetryAddedDomainEvent
        {
            SensorId = Guid.NewGuid(),
            EcosystemId = Guid.NewGuid(),
            Period = PeriodType.Minute,
            MinValue = 10.0,
            MaxValue = 30.0,
            AvgValue = 20.0,
            Time = DateTime.UtcNow
        };

        var expectedPointDto = new TelemetryChartPointDto
        {
            SensorId = domainEvent.SensorId,
            MinValue = domainEvent.MinValue,
            MaxValue = domainEvent.MaxValue,
            AvgValue = domainEvent.AvgValue,
            Time = domainEvent.Time
        };

        _mapperMock.Map<TelemetryChartPointDto>(domainEvent).Returns(expectedPointDto);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await _realtimeNotifierMock.Received(1).AggregatePointGenerated(
            domainEvent.EcosystemId.ToString(),
            domainEvent.Period,
            Arg.Is<TelemetryChartPointDto>(p => p.SensorId == expectedPointDto.SensorId &&
                                               Math.Abs(p.MinValue - expectedPointDto.MinValue) < 0.001 &&
                                               Math.Abs(p.MaxValue - expectedPointDto.MaxValue) < 0.001 &&
                                               Math.Abs(p.AvgValue - expectedPointDto.AvgValue) < 0.001 &&
                                               p.Time == expectedPointDto.Time));
    }
}
