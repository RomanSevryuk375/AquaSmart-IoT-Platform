using AutoMapper;
using Contracts.Events.ControllerEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.Features.Alerts.Commands.SendControllerOfflineAlert;

namespace Notification.Infrastructure.Messaging;

public sealed class ControllerNotOnlineEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<ControllerNotOnlineEvent>
{
    public async Task Consume(ConsumeContext<ControllerNotOnlineEvent> context)
    {
        SendControllerOfflineAlertCommand command = mapper.Map<SendControllerOfflineAlertCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
