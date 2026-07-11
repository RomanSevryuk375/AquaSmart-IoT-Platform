using AutoMapper;
using MediatR;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Events;

namespace Telemetry.Application.Handlers;

public sealed class AggregatedTelemetryAddedEventHandler(ITelemetryNotifier realTimeNotifier, IMapper mapper)
    : INotificationHandler<AggregatedTelemetryAddedDomainEvent>
{
    public async Task Handle(AggregatedTelemetryAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        TelemetryChartPointDto point = mapper.Map<TelemetryChartPointDto>(notification);
        await realTimeNotifier.AggregatePointGenerated(
            notification.EcosystemId.ToString(), notification.Period, point);
    }
}
