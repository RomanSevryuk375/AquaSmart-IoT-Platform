using AutoMapper;
using Contracts.Events.UserEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Application.Features.Users.Commands.SyncUserCreated;

namespace Notification.Infrastructure.Messaging;

public sealed class UserUpdatedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<UserUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        SyncUserUpdateCommand command = mapper.Map<SyncUserUpdateCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
