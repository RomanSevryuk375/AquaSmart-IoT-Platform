// Ignore Spelling: ttl

using System.Text.Json;
using Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using TelegramBot.Domain.Fsm;

namespace TelegramBotService.Infrastructure.Sessions;

/// <summary>
/// Redis-backed implementation of <see cref="IFsmSessionStore"/>.
/// Keys are stored under <c>fsm:{chatId}</c> with a configurable TTL (default 15 minutes).
/// </summary>
public sealed class RedisFsmSessionStore(IDistributedCache cache) : IFsmSessionStore
{
    private static readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(15);

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task SaveAsync(
        long chatId,
        DialogueSessionData data,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        string json = JsonSerializer.Serialize(data, _jsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
        };

        await cache.SetStringAsync(BuildKey(chatId), json, options, cancellationToken);
    }

    public async Task<DialogueSessionData> GetAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        string? json = await cache.GetStringAsync(BuildKey(chatId), cancellationToken);
        if (string.IsNullOrEmpty(json))
        {
            return new DialogueSessionData { State = UserDialogueState.None };
        }

        return JsonSerializer.Deserialize<DialogueSessionData>(json, _jsonOptions)
               ?? new DialogueSessionData { State = UserDialogueState.None };
    }

    public async Task DeleteAsync(long chatId, CancellationToken cancellationToken = default) =>
        await cache.RemoveAsync(BuildKey(chatId), cancellationToken);

    private static string BuildKey(long chatId) => $"fsm:{chatId}";
}
