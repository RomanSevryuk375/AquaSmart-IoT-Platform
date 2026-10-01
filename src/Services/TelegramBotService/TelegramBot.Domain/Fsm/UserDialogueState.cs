namespace TelegramBot.Domain.Fsm;

/// <summary>
/// FSM states for the user dialogue flow in the Telegram bot.
/// Each state represents what input the bot is currently waiting for.
/// </summary>
public enum UserDialogueState
{
    /// <summary>User is in the main menu — no active dialogue.</summary>
    None = 0,

    // ── Maintenance Log flow ──────────────────────────────────────

    /// <summary>Bot has asked the user to select an ecosystem for the log entry.</summary>
    Log_WaitingForEcosystem = 10,

    /// <summary>Ecosystem selected; waiting for the pH value.</summary>
    Log_WaitingForPh = 11,

    /// <summary>pH received; waiting for the KH (carbonate hardness) value.</summary>
    Log_WaitingForKh = 12,

    /// <summary>KH received; waiting for the NO₃ (nitrate) value.</summary>
    Log_WaitingForNo3 = 13,

    /// <summary>All metrics received; waiting for the free-form notes.</summary>
    Log_WaitingForNotes = 14,

    // ── Reminder creation flow ────────────────────────────────────

    /// <summary>Bot has asked the user to select an ecosystem for the reminder.</summary>
    Reminder_WaitingForEcosystem = 20,

    /// <summary>Ecosystem selected; waiting for the task name (e.g. "Water change 30%").</summary>
    Reminder_WaitingForTaskName = 21,

    /// <summary>Task name received; waiting for the interval in days.</summary>
    Reminder_WaitingForInterval = 22
}
