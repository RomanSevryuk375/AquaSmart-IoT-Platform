using BuildingBlocks.Domain.Results;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Users.Commands.SyncTelegramAccountLinked;

public sealed class TelegramAccountLinkedHandler(
    IUserRepository userRepository) : IRequestHandler<TelegramAccountLinkedCommand, Result>
{
    public async Task<Result> Handle(
        TelegramAccountLinkedCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.NotFound<User>(
                $"User {request.UserId} not found."));
        }

        // Idempotency: event could be delivered more than once (at-least-once semantics).
        // If the user already has this exact ChatId, the sync is already done — return success silently.
        if (user.TelegramChatId == request.TelegramChatId)
        {
            return Result.Success();
        }

        if (await userRepository.TelegramChatIdExistsAsync(request.TelegramChatId, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Telegram.AlreadyLinked",
                "This Telegram account is already linked to another user."));
        }

        user.LinkTelegramChat(request.TelegramChatId);

        return Result.Success();
    }
}
