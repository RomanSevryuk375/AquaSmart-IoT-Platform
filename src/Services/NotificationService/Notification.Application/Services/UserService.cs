using Contracts.Events.UserEvents;
using Contracts.Results;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public sealed class UserService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IUserService
{
    public async Task<ConsumerResult> CreateUserAsync(
        UserCreatedEvent userCreated,
        CancellationToken cancellationToken)
    {
        var existingUser = await userRepository
            .GetByIdAsync(userCreated.UserId, cancellationToken);

        if (existingUser is not null)
        {
            return ConsumerResult.Success();
        }

        var (user, errors) = UserEntity.Create(
            userCreated.UserId,
            userCreated.Email,
            userCreated.TimeZone,
            userCreated.PhoneNumber,
            false,
            false,
            null,
            false,
            userCreated.CreatedAt);

        if (user is null)
        {
            return ConsumerResult
                .FatalError($"Failed to create {nameof(UserEntity)}: {string.Join(", ", errors)}");
        }

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> UpdateUserAsync(
        UserUpdatedEvent user,
        CancellationToken cancellationToken)
    {
        var currentUser = await userRepository
            .GetByIdAsync(user.UserId, cancellationToken);

        if (currentUser is null)
        {
            return ConsumerResult
                .RetryableError($"User {user.UserId} not found");
        }

        var errors = currentUser.UpdateContacts(user.Email, user.PhoneNumber);

        if (errors is not null)
        {
            return ConsumerResult
                .FatalError($"Failed to update {nameof(UserEntity)}: {string.Join(", ", errors)}");
        }

        await userRepository.UpdateAsync(currentUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
