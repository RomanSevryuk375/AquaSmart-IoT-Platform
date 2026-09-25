using BuildingBlocks.Domain.Results;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MediatR;
using ZiggyCreatures.Caching.Fusion;

namespace IdentityService.Application.Features.Auth.Commands.VerifyTelegram;

public sealed class VerifyTelegramHandler(
    IFusionCache cache,
    IUserRepository userRepository) : IRequestHandler<VerifyTelegramCommand, Result>
{
    public async Task<Result> Handle(VerifyTelegramCommand request, CancellationToken cancellationToken)
    {
        string cacheKey = $"tg-link:{request.Token}";

        string? rawUserId = await cache.GetOrDefaultAsync<string>(cacheKey, token: cancellationToken);
        if (string.IsNullOrWhiteSpace(rawUserId) || !Guid.TryParse(rawUserId, out Guid userId))
        {
            return Result.Failure(Error.NotFound(
                "TelegramLink.InvalidOrExpired",
                "Link token is invalid or has expired."));
        }

        if (await userRepository.TelegramChatIdExistsAsync(request.ChatId, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Telegram.AlreadyLinked",
                "This Telegram account is already linked to another user."));
        }

        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.NotFound<User>($"User {userId} not found."));
        }

        await cache.RemoveAsync(cacheKey, token: cancellationToken);

        user.LinkTelegramChat(request.ChatId);

        return Result.Success();
    }
}
