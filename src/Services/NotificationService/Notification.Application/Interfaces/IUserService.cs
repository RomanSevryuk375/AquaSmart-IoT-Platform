using Contracts.Events.UserEvents;
using Contracts.Results;

namespace Notification.Application.Interfaces;

public interface IUserService
{
    Task<ConsumerResult> CreateUserAsync(
        UserCreatedEvent userCreated, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> UpdateUserAsync(
        UserUpdatedEvent user,
        CancellationToken cancellationToken);
}