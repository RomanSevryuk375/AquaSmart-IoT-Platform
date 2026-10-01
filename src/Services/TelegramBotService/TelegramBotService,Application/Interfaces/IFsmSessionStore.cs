using TelegramBot.Domain.Fsm;

namespace Interfaces;

/// <summary>
/// Redis-backed store for per-user FSM dialogue sessions.
/// </summary>
public interface IFsmSessionStore
{
    /// <summary>
    /// Saves (or replaces) the session data for the given chat.
    /// The entry automatically expires after <paramref name="ttl"/>.
    /// </summary>
    public Task SaveAsync(
        long chatId,
        DialogueSessionData data,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current session data for the given chat.
    /// Returns a default <see cref="DialogueSessionData"/> with <see cref="UserDialogueState.None"/>
    /// when no session exists.
    /// </summary>
    public Task<DialogueSessionData> GetAsync(
        long chatId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the session — call after a flow completes or the user hits Cancel.
    /// </summary>
    public Task DeleteAsync(
        long chatId,
        CancellationToken cancellationToken = default);
}
