namespace TelegramBot.Domain.Fsm;

/// <summary>
/// Accumulates user input across multiple FSM steps.
/// Stored in Redis under the key <c>fsm:{chatId}</c> with a configurable TTL.
/// </summary>
public sealed record DialogueSessionData
{
    // ── State ────────────────────────────────────────────────────

    /// <summary>Current FSM state — what the bot is waiting for next.</summary>
    public UserDialogueState State { get; init; } = UserDialogueState.None;

    // ── Shared ───────────────────────────────────────────────────

    /// <summary>Ecosystem ID chosen by the user (shared between Log and Reminder flows).</summary>
    public Guid? SelectedEcosystemId { get; init; }

    /// <summary>Ecosystem display name — shown back in confirmation messages.</summary>
    public string? SelectedEcosystemName { get; init; }

    // ── Maintenance Log ──────────────────────────────────────────

    /// <summary>pH value entered by the user.</summary>
    public double? Ph { get; init; }

    /// <summary>Carbonate hardness (KH) value entered by the user.</summary>
    public double? Kh { get; init; }

    /// <summary>Nitrate (NO₃) value entered by the user.</summary>
    public double? No3 { get; init; }

    // ── Reminder ─────────────────────────────────────────────────

    /// <summary>Name of the recurring task (e.g. "Water change 30%").</summary>
    public string? ReminderTaskName { get; init; }

    /// <summary>How many days between occurrences.</summary>
    public int? ReminderIntervalDays { get; init; }
}
