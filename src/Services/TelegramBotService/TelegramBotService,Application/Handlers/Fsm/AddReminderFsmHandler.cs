using DTOs;
using Interfaces;
using TelegramBot.Domain.Fsm;

namespace Handlers.Fsm;

/// <summary>
/// Orchestrates the multi-step FSM flow for creating a new reminder.
///
/// State machine:
///   None
///     → Reminder_WaitingForEcosystem   (ask user to pick ecosystem)
///     → Reminder_WaitingForTaskName    (ask task name)
///     → Reminder_WaitingForInterval    (ask interval in days)
///     → None                           (submit reminder, confirm)
/// </summary>
public sealed class AddReminderFsmHandler(
    IFsmSessionStore sessionStore,
    INotificationApiClient notificationClient,
    IControlApiClient controlClient,
    ITelegramResponseService response)
{
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(15);

    // ── Entry point ──────────────────────────────────────────────

    /// <summary>Called when user sends /add_reminder or taps "➕ Напоминание".</summary>
    public async Task StartAsync(long chatId, CancellationToken cancellationToken)
    {
        IReadOnlyList<EcosystemDto> ecosystems =
            await controlClient.GetUserEcosystemsAsync(chatId, cancellationToken);

        if (ecosystems.Count == 0)
        {
            await response.SendTextAsync(chatId,
                "🐠 У тебя пока нет аквариумов. Создай экосистему в приложении AquaSmart.",
                cancellationToken);
            return;
        }

        await sessionStore.SaveAsync(
            chatId,
            new DialogueSessionData { State = UserDialogueState.Reminder_WaitingForEcosystem },
            SessionTtl,
            cancellationToken);

        object keyboard = BuildEcosystemKeyboard(ecosystems);

        await response.SendWithKeyboardAsync(
            chatId,
            "⏰ <b>Новое напоминание</b>\n\nВыбери аквариум:",
            keyboard,
            cancellationToken);
    }

    // ── Dispatcher ───────────────────────────────────────────────

    public async Task HandleInputAsync(
        long chatId,
        string input,
        DialogueSessionData session,
        CancellationToken cancellationToken)
    {
        if (input.Equals("Отмена", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("/cancel", StringComparison.OrdinalIgnoreCase))
        {
            await CancelAsync(chatId, cancellationToken);
            return;
        }

        await (session.State switch
        {
            UserDialogueState.Reminder_WaitingForEcosystem => HandleEcosystemAsync(chatId, input, session, cancellationToken),
            UserDialogueState.Reminder_WaitingForTaskName => HandleTaskNameAsync(chatId, input, session, cancellationToken),
            UserDialogueState.Reminder_WaitingForInterval => HandleIntervalAsync(chatId, input, session, cancellationToken),
            _ => Task.CompletedTask
        });
    }

    // ── Step handlers ─────────────────────────────────────────────

    private async Task HandleEcosystemAsync(
        long chatId, string input, DialogueSessionData session, CancellationToken cancellationToken)
    {
        if (!TryParseEcosystemCallback(input, out Guid ecosystemId, out string ecosystemName))
        {
            await response.SendTextAsync(chatId, "❓ Пожалуйста, выбери аквариум из списка выше.", cancellationToken);
            return;
        }

        await sessionStore.SaveAsync(chatId, session with
        {
            State = UserDialogueState.Reminder_WaitingForTaskName,
            SelectedEcosystemId = ecosystemId,
            SelectedEcosystemName = ecosystemName
        }, SessionTtl, cancellationToken);

        await response.SendWithKeyboardAsync(chatId,
            $"🌿 <b>Аквариум: {ecosystemName}</b>\n\nВведи название задачи (например: <code>Подмена воды 30%</code>):",
            CancelKeyboard(),
            cancellationToken);
    }

    private async Task HandleTaskNameAsync(
        long chatId, string input, DialogueSessionData session, CancellationToken cancellationToken)
    {
        string taskName = input.Trim();
        if (taskName.Length < 2 || taskName.Length > 120)
        {
            await response.SendTextAsync(chatId,
                "❌ Название задачи должно быть от 2 до 120 символов. Попробуй снова:",
                cancellationToken);
            return;
        }

        await sessionStore.SaveAsync(chatId, session with
        {
            State = UserDialogueState.Reminder_WaitingForInterval,
            ReminderTaskName = taskName
        }, SessionTtl, cancellationToken);

        await response.SendWithKeyboardAsync(chatId,
            $"✅ Задача: <b>{taskName}</b>\n\n" +
            $"Введи периодичность в <b>днях</b> (например: <code>7</code> для еженедельного):",
            CancelKeyboard(),
            cancellationToken);
    }

    private async Task HandleIntervalAsync(
        long chatId, string input, DialogueSessionData session, CancellationToken cancellationToken)
    {
        if (!int.TryParse(input.Trim(), out int days) || days < 1 || days > 3650)
        {
            await response.SendTextAsync(chatId,
                "❌ Введи целое число дней от 1 до 3650 (например: 7).",
                cancellationToken);
            return;
        }

        var request = new CreateReminderRequest
        {
            EcosystemId = session.SelectedEcosystemId!.Value,
            TaskName = session.ReminderTaskName!,
            IntervalDays = days
        };

        _ = await notificationClient.CreateReminderAsync(chatId, request, cancellationToken);
        await sessionStore.DeleteAsync(chatId, cancellationToken);

        DateTime nextDue = DateTime.UtcNow.AddDays(days);

        string confirmation =
            $"✅ <b>Напоминание создано!</b>\n\n" +
            $"⏰ <b>{session.ReminderTaskName}</b>\n" +
            $"🌿 <b>Аквариум:</b> {session.SelectedEcosystemName}\n" +
            $"📅 <b>Первый срок:</b> {nextDue:dd.MM.yyyy}\n" +
            $"🔄 <b>Интервал:</b> каждые {days} дн.";

        await response.SendTextAsync(chatId, confirmation, cancellationToken);
    }

    private async Task CancelAsync(long chatId, CancellationToken cancellationToken)
    {
        await sessionStore.DeleteAsync(chatId, cancellationToken);
        await response.SendTextAsync(chatId, "❌ Создание напоминания отменено.", cancellationToken);
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static object BuildEcosystemKeyboard(IReadOnlyList<EcosystemDto> ecosystems)
    {
        var rows = ecosystems
            .Chunk(2)
            .Select(chunk => chunk
                .Select(e => new
                {
                    text = e.Name,
                    callback_data = $"ecosystem:{e.Id}:{e.Name}"
                })
                .ToArray())
            .Append(new[] { new { text = "❌ Отмена", callback_data = "cancel_flow" } })
            .ToArray();

        return new { inline_keyboard = rows };
    }

    private static object CancelKeyboard() => new
    {
        inline_keyboard = new[]
        {
            new[] { new { text = "❌ Отмена", callback_data = "cancel_flow" } }
        }
    };

    private static bool TryParseEcosystemCallback(
        string input, out Guid id, out string name)
    {
        id = Guid.Empty;
        name = string.Empty;

        if (!input.StartsWith("ecosystem:", StringComparison.Ordinal))
        {
            return false;
        }

        string[] parts = input.Split(':', 3);
        if (parts.Length < 3)
        {
            return false;
        }

        if (!Guid.TryParse(parts[1], out id))
        {
            return false;
        }

        name = parts[2];
        return true;
    }
}
