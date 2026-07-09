using AutoMapper;
using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.Features.Alerts.Commands.SendTelemetryAlert;

namespace Notification.Infrastructure.Messaging;

public sealed class CriticalTelemetryThresholdAlertEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<CriticalTelemetryThresholdAlertEvent>
{
    public async Task Consume(ConsumeContext<CriticalTelemetryThresholdAlertEvent> context)
    {
        SendTelemetryAlertCommand command = mapper.Map<SendTelemetryAlertCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
