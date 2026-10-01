namespace Interfaces;

/// <summary>
/// Application-layer abstraction over IdentityService HMAC calls.
/// Implemented in Infrastructure by wrapping <c>IdentityHttpClient</c>.
/// </summary>
public interface IIdentityClient
{
    /// <summary>Verify a deep-link token and associate the chatId with an AquaSmart account.</summary>
    public Task<bool> VerifyLinkTokenAsync(string token, long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve a Bearer JWT for the account associated with this chatId.
    /// Returns <c>null</c> if the chatId is not linked.
    /// </summary>
    public Task<string?> LoginByChatIdAsync(long chatId, CancellationToken cancellationToken = default);
}
