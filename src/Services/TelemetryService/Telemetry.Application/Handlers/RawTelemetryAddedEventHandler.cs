using AutoMapper;
using Contracts.Events.TelemetryEvents;
using MassTransit;
using MediatR;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Events;

namespace Telemetry.Application.Handlers;

public sealed class RawTelemetryAddedEventHandler(
    ITelemetryNotifier realtimeNotifier,
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : INotificationHandler<RawTelemetryAddedDomainEvent>
{
    public async Task Handle(RawTelemetryAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        TelemetryRawChartPointDto pointDto = mapper.Map<TelemetryRawChartPointDto>(notification);
        await realtimeNotifier.TelemetryRawReceived(notification.EcosystemId.ToString(), pointDto);

        TelemetryReceivedEvent integrationEvent = mapper.Map<TelemetryReceivedEvent>(notification);
        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
