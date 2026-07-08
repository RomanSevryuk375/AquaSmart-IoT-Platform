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
        User? existingUser = await userRepository
            .GetByIdAsync(userCreated.UserId, cancellationToken);

        if (existingUser is not null)
        {
            return ConsumerResult.Success();
        }

        Result<User>? userResult = User.Create(
            userCreated.UserId,
            userCreated.Email,
            userCreated.TimeZone,
            userCreated.PhoneNumber,
            false,
            false,
            null,
            false,
            userCreated.CreatedAt);

        if (userResult.IsFailure)
        {
            return ConsumerResult.FatalError(userResult.Error.Message);
        }

        await userRepository.AddAsync(userResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> UpdateUserAsync(
        UserUpdatedEvent user,
        CancellationToken cancellationToken)
    {
        User? currentUser = await userRepository
            .GetByIdAsync(user.UserId, cancellationToken);

        if (currentUser is null)
        {
            return ConsumerResult
                .RetryableError($"User {user.UserId} not found");
        }

        Result? errors = currentUser.UpdateContacts(user.Email, user.PhoneNumber);

        if (errors is not null)
        {
            return ConsumerResult
                .FatalError($"Failed to update {nameof(User)}: {string.Join(", ", errors)}");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
