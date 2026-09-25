using BuildingBlocks.Domain.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Options;
using MediatR;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace IdentityService.Application.Features.Auth.Commands.GenerateTelegramLink;

public sealed class GenerateTelegramLinkHandler(IFusionCache cache, IOptions<TelegramBotOptions> botOptions)
    : IRequestHandler<GenerateTelegramLinkCommand, Result<TelegramLinkTokenResponseDto>>
{
    private static readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(10);

    public async Task<Result<TelegramLinkTokenResponseDto>> Handle(
        GenerateTelegramLinkCommand request,
        CancellationToken cancellationToken)
    {
        string token = Guid.NewGuid().ToString("N");
        string cacheKey = $"tg-link:{token}";
        DateTime expireAt = DateTime.UtcNow.Add(_tokenLifetime);

        await cache.SetAsync(
            cacheKey,
            request.UserId.ToString(),
            new FusionCacheEntryOptions { Duration = _tokenLifetime },
            cancellationToken);

        string botUsername = botOptions.Value.Name.TrimStart('@');

        string link = $"https://t.me/{botUsername}?start={token}";

        return Result<TelegramLinkTokenResponseDto>.Success(new TelegramLinkTokenResponseDto
        {
            Link = link,
            ExpireAt = expireAt,
        });
    }
}
