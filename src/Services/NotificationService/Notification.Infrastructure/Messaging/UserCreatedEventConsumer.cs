using AutoMapper;
using Contracts.Events.UserEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.Features.Users.Commands.SyncUserCreated;

namespace Notification.Infrastructure.Messaging;

public sealed class UserCreatedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        SyncUserUpdateCommand command = mapper.Map<SyncUserUpdateCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
