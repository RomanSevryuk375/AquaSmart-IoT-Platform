using DTOs;
using Interfaces;
using TelegramBot.Domain.Fsm;

namespace Handlers.Fsm;

/// <summary>
/// Orchestrates the multi-step FSM flow for adding a maintenance log.
///
/// State machine:
///   None
///     → Log_WaitingForEcosystem  (ask user to pick ecosystem)
///     → Log_WaitingForPh         (ask pH)
///     → Log_WaitingForKh         (ask KH)
///     → Log_WaitingForNo3        (ask NO₃)
///     → Log_WaitingForNotes      (ask notes)
///     → None                     (submit log, confirm)
/// </summary>
public sealed class AddLogFsmHandler(
    IFsmSessionStore sessionStore,
    INotificationApiClient notificationClient,
    IControlApiClient controlClient,
    ITelegramResponseService response)
{
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(15);

    // ── Entry point ──────────────────────────────────────────────

    /// <summary>Called when user sends /add_log or taps "📝 Внести лог".</summary>
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
            new DialogueSessionData { State = UserDialogueState.Log_WaitingForEcosystem },
            SessionTtl,
            cancellationToken);

        object keyboard = BuildEcosystemKeyboard(ecosystems);

        await response.SendWithKeyboardAsync(
            chatId,
            "📝 <b>Лог обслуживания</b>\n\nВыбери аквариум:",
            keyboard,
            cancellationToken);
    }

    // ── State handlers ───────────────────────────────────────────

    /// <summary>Dispatches the incoming message to the correct state handler.</summary>
    public async Task HandleInputAsync(
        long chatId,
        string input,
        DialogueSessionData session,
        CancellationToken cancellationToken)
    {
        // Cancel shortcut
        if (input.Equals("Отмена", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("/cancel", StringComparison.OrdinalIgnoreCase))
        {
            await CancelAsync(chatId, cancellationToken);
            return;
        }

        await (session.State switch
        {
            UserDialogueState.Log_WaitingForEcosystem => HandleEcosystemAsync(chatId, input, session, cancellationToken),
            UserDialogueState.Log_WaitingForPh => HandlePhAsync(chatId, input, session, cancellationToken),
            UserDialogueState.Log_WaitingForKh => HandleKhAsync(chatId, input, session, cancellationToken),
            UserDialogueState.Log_WaitingForNo3 => HandleNo3Async(chatId, input, session, cancellationToken),
            UserDialogueState.Log_WaitingForNotes => HandleNotesAsync(chatId, input, session, cancellationToken),
            _ => Task.CompletedTask
        });
    }

    // ── Private step handlers ─────────────────────────────────────

    private async Task HandleEcosystemAsync(
        long chatId, string input, DialogueSessionData session, CancellationToken cancellationToken)
    {
        // Input is the ecosystem name from the inline button callback: "ecosystem:{id}:{name}"
        if (!TryParseEcosystemCallback(input, out Guid ecosystemId, out string ecosystemName))
        {
            await response.SendTextAsync(chatId, "❓ Пожалуйста, выбери аквариум из списка выше.", cancellationToken);
            return;
        }

        await sessionStore.SaveAsync(chatId, session with
        {
            State = UserDialogueState.Log_WaitingForPh,
            SelectedEcosystemId = ecosystemId,
            SelectedEcosystemName = ecosystemName
        }, SessionTtl, cancellationToken);

        await response.SendWithKeyboardAsync(chatId,
            $"🌡 <b>Аквариум: {ecosystemName}</b>\n\nВведи значение <b>pH</b> (например: <code>7.2</code>):",
            CancelKeyboard(),
            cancellationToken);
    }

    private async Task HandlePhAsync(
        long chatId, string input, DialogueSessionData session, CancellationToken cancellationToken)
    {
        if (!double.TryParse(input.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double ph) || ph < 0 || ph > 14)
        {
            await response.SendTextAsync(chatId, "❌ Некорректное значение pH. Введи число от 0 до 14 (например: 7.2).", cancellationToken);
            return;
        }

        await sessionStore.SaveAsync(chatId, session with
        {
            State = UserDialogueState.Log_WaitingForKh,
            Ph = ph
        }, SessionTtl, cancellationToken);

        await response.SendWithKeyboardAsync(chatId,
            $"✅ pH: <b>{ph}</b>\n\nТеперь введи <b>KH</b> (карбонатная жёсткость, °dKH, например: <code>8</code>):",
            CancelKeyboard(),
            cancellationToken);
    }

    private async Task HandleKhAsync(
        long chatId, string input, DialogueSessionData session, CancellationToken cancellationToken)
    {
        if (!double.TryParse(input.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double kh) || kh < 0)
        {
            await response.SendTextAsync(chatId, "❌ Некорректное значение KH. Введи положительное число (например: 8).", cancellationToken);
            return;
        }

        await sessionStore.SaveAsync(chatId, session with
        {
            State = UserDialogueState.Log_WaitingForNo3,
            Kh = kh
        }, SessionTtl, cancellationToken);

        await response.SendWithKeyboardAsync(chatId,
            $"✅ KH: <b>{kh}</b>\n\nТеперь введи <b>NO₃</b> (нитраты, мг/л, например: <code>15</code>):",
            CancelKeyboard(),
            cancellationToken);
    }

    private async Task HandleNo3Async(
        long chatId, string input, DialogueSessionData session, CancellationToken cancellationToken)
    {
        if (!double.TryParse(input.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double no3) || no3 < 0)
        {
            await response.SendTextAsync(chatId, "❌ Некорректное значение NO₃. Введи положительное число (например: 15).", cancellationToken);
            return;
        }

        await sessionStore.SaveAsync(chatId, session with
        {
            State = UserDialogueState.Log_WaitingForNotes,
            No3 = no3
        }, SessionTtl, cancellationToken);

        await response.SendWithKeyboardAsync(chatId,
            $"✅ NO₃: <b>{no3} мг/л</b>\n\nДобавь заметку (или нажми «Пропустить»):",
            SkipOrCancelKeyboard(),
            cancellationToken);
    }

    private async Task HandleNotesAsync(
        long chatId, string input, DialogueSessionData session, CancellationToken cancellationToken)
    {
        string notes = input.Equals("Пропустить", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : input.Trim();

        // Submit the log
        var request = new CreateMaintenanceLogRequest
        {
            EcosystemId = session.SelectedEcosystemId!.Value,
            ActionDate = DateTime.UtcNow,
            Metrics = new Dictionary<string, double>
            {
                ["pH"] = session.Ph ?? 0,
                ["KH"] = session.Kh ?? 0,
                ["NO3"] = session.No3 ?? 0
            },
            Notes = notes
        };

        await notificationClient.CreateMaintenanceLogAsync(chatId, request, cancellationToken);
        await sessionStore.DeleteAsync(chatId, cancellationToken);

        string confirmation =
            $"📋 <b>Лог обслуживания сохранён!</b>\n\n" +
            $"🌿 <b>Аквариум:</b> {session.SelectedEcosystemName}\n" +
            $"📅 <b>Дата:</b> {DateTime.UtcNow:dd.MM.yyyy HH:mm}\n" +
            $"📊 <b>Параметры:</b> pH: {session.Ph} | KH: {session.Kh} | NO₃: {session.No3} мг/л\n" +
            (string.IsNullOrEmpty(notes) ? string.Empty : $"📝 <b>Заметка:</b> {notes}");

        await response.SendTextAsync(chatId, confirmation, cancellationToken);
    }

    private async Task CancelAsync(long chatId, CancellationToken cancellationToken)
    {
        await sessionStore.DeleteAsync(chatId, cancellationToken);
        await response.SendTextAsync(chatId, "❌ Ввод отменён. Возвращаюсь в главное меню.", cancellationToken);
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static object BuildEcosystemKeyboard(IReadOnlyList<EcosystemDto> ecosystems)
    {
        // Build rows of 2 ecosystem buttons each
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

    private static object SkipOrCancelKeyboard() => new
    {
        inline_keyboard = new[]
        {
            new[]
            {
                new { text = "⏭ Пропустить", callback_data = "log_skip_notes" },
                new { text = "❌ Отмена",     callback_data = "cancel_flow" }
            }
        }
    };

    private static bool TryParseEcosystemCallback(
        string input, out Guid id, out string name)
    {
        id = Guid.Empty;
        name = string.Empty;

        // Expected format: "ecosystem:{guid}:{name}"
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
