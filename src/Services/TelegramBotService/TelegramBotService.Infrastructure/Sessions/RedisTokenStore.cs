// Ignore Spelling: ttl

using Microsoft.Extensions.Caching.Distributed;

namespace TelegramBotService.Infrastructure.Sessions;

/// <summary>
/// Stores and retrieves Bearer JWT tokens for Telegram users.
/// Key: <c>jwt:{chatId}</c>. TTL matches token expiry (default 24h).
/// </summary>
public sealed class RedisTokenStore(IDistributedCache cache)
{
    private static readonly TimeSpan _defaultTtl = TimeSpan.FromHours(24);

    public async Task SaveTokenAsync(
        long chatId,
        string token,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
        };

        await cache.SetStringAsync(BuildKey(chatId), token, options, cancellationToken);
    }

    public async Task<string?> GetTokenAsync(long chatId, CancellationToken cancellationToken = default) =>
        await cache.GetStringAsync(BuildKey(chatId), cancellationToken);

    public async Task DeleteTokenAsync(long chatId, CancellationToken cancellationToken = default) =>
        await cache.RemoveAsync(BuildKey(chatId), cancellationToken);

    private static string BuildKey(long chatId) => $"jwt:{chatId}";
}
