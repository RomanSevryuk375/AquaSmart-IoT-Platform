using Interfaces;
using TelegramBotService.Infrastructure.HttpClients;
using TelegramBotService.Infrastructure.Sessions;

namespace TelegramBotService.Infrastructure.Adapters;

/// <summary>
/// Adapts the existing <see cref="IdentityHttpClient"/> (HMAC-signed S2S client)
/// to the application-layer <see cref="IIdentityClient"/> contract.
/// After a successful login, the JWT is persisted in Redis for subsequent API calls.
/// </summary>
internal sealed class IdentityClientAdapter(
    IdentityHttpClient identityHttpClient,
    RedisTokenStore tokenStore) : IIdentityClient
{
    public async Task<bool> VerifyLinkTokenAsync(
        string token,
        long chatId,
        CancellationToken cancellationToken = default)
    {
        bool success = await identityHttpClient.VerifyLinkTokenAsync(token, chatId, cancellationToken);

        if (success)
        {
            string? jwt = await identityHttpClient.LoginByChatIdAsync(chatId, cancellationToken);
            if (!string.IsNullOrEmpty(jwt))
            {
                await tokenStore.SaveTokenAsync(chatId, jwt, cancellationToken: cancellationToken);
            }
        }

        return success;
    }

    public async Task<string?> LoginByChatIdAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        string? cached = await tokenStore.GetTokenAsync(chatId, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        string? jwt = await identityHttpClient.LoginByChatIdAsync(chatId, cancellationToken);
        if (!string.IsNullOrEmpty(jwt))
        {
            await tokenStore.SaveTokenAsync(chatId, jwt, cancellationToken: cancellationToken);
        }

        return jwt;
    }
}
