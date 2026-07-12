using AutoMapper;
using Contracts.Events.TelemetryEvents;
using MediatR;
using Notification.Application.Features.Alerts.Commands.SendTelemetryAlert;

namespace Notification.Infrastructure.Messaging.Alert;

internal sealed class CriticalTelemetryThresholdAlertEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<CriticalTelemetryThresholdAlertEvent, SendTelemetryAlertCommand>(sender, mapper);

