using Contracts.Results;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Users.Commands.SyncUserCreated;

public sealed class SyncUserCreatedHandler(IUserRepository userRepository)
    : IRequestHandler<SyncUserUpdateCommand, Result>
{
    public async Task<Result> Handle(SyncUserUpdateCommand request, CancellationToken cancellationToken)
    {
        User? existingUser = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (existingUser is not null)
        {
            return Result.Success();
        }

        Result<User>? userResult = User.Create(
            request.UserId,
            request.Email,
            request.TimeZone,
            request.PhoneNumber,
            emailEnable: false,
            tgEnable: false,
            telegramChatId: null,
            enable: false,
            request.CreatedAt);
        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Error);
        }

        await userRepository.AddAsync(userResult.Value, cancellationToken);

        return Result.Success();
    }
}
