using AutoMapper;
using Contracts.Events.UserEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.Features.Alerts.Commands.SendSubscriptionAlert;

namespace Notification.Infrastructure.Messaging;

public sealed class SubscriptionDowngradedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<SubscriptionDowngradedEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionDowngradedEvent> context)
    {
        SendSubscriptionAlertCommand command = mapper.Map<SendSubscriptionAlertCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
