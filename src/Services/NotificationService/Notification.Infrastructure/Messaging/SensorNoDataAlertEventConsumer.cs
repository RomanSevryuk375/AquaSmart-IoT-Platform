using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.Features.Alerts.Commands.SendSensorNoDataAlert;

namespace Notification.Infrastructure.Messaging;

public sealed class SensorNoDataAlertEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<SensorNoDataAlertEvent>
{
    public async Task Consume(ConsumeContext<SensorNoDataAlertEvent> context)
    {
        SendSensorNoDataAlertCommand command = mapper.Map<SendSensorNoDataAlertCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
