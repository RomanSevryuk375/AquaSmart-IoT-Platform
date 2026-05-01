using Contracts.Events.UserEvents;
using MassTransit;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Messaging;

public sealed class UserUpdatedEventConsumer(
    IUserService userService) : IConsumer<UserUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var result = await userService.UpdateUserAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
