using System.Text.Json;
using Handlers.Callbacks;
using Handlers.Commands;
using Handlers.Fsm;
using Interfaces;
using Microsoft.Extensions.Logging;
using TelegramBot.Domain.Fsm;

namespace TelegramBotService.Infrastructure.Telegram;

/// <summary>
/// Routes incoming Telegram Update objects to the correct application handler.
///
/// Supported update types:
///   • Message  → CommandRouter or FsmMessageHandler
///   • CallbackQuery → CallbackRouter
/// </summary>
public sealed class TelegramUpdateDispatcher(
    StartCommandHandler startHandler,
    ShowRemindersHandler remindersHandler,
    AddLogFsmHandler addLogHandler,
    AddReminderFsmHandler addReminderHandler,
    MarkNotificationReadCallbackHandler markReadHandler,
    CompleteReminderCallbackHandler completeReminderHandler,
    CancelFlowCallbackHandler cancelFlowHandler,
    IFsmSessionStore sessionStore,
    ILogger<TelegramUpdateDispatcher> logger)
{
    /// <summary>
    /// Entry point — parses the Telegram update JSON and dispatches accordingly.
    /// </summary>
    public async Task HandleUpdateAsync(JsonElement update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.TryGetProperty("message", out JsonElement message))
            {
                await HandleMessageAsync(message, cancellationToken);
                return;
            }

            if (update.TryGetProperty("callback_query", out JsonElement callbackQuery))
            {
                await HandleCallbackQueryAsync(callbackQuery, cancellationToken);
                return;
            }

            logger.LogDebug("Unhandled update type");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Telegram update");
        }
    }

    // ── Message routing ───────────────────────────────────────────

    private async Task HandleMessageAsync(JsonElement message, CancellationToken cancellationToken)
    {
        if (!TryExtractChatId(message, out long chatId))
        {
            return;
        }

        string text = message.TryGetProperty("text", out JsonElement textEl)
            ? textEl.GetString() ?? string.Empty
            : string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        // ── Named commands (start with /) ─────────────────────────
        if (text.StartsWith('/'))
        {
            string[] parts = text.Split(' ', 2);
            string command = parts[0].ToLowerInvariant();
            string? payload = parts.Length > 1 ? parts[1] : null;

            switch (command)
            {
                case "/start":
                    await startHandler.HandleAsync(chatId, payload, cancellationToken);
                    return;

                case "/reminders":
                    await remindersHandler.HandleAsync(chatId, cancellationToken);
                    return;

                case "/add_log":
                    await addLogHandler.StartAsync(chatId, cancellationToken);
                    return;

                case "/add_reminder":
                    await addReminderHandler.StartAsync(chatId, cancellationToken);
                    return;

                case "/cancel":
                    // Cancel handled by FSM if a session exists
                    break;
            }
        }

        // ── Reply keyboard buttons ────────────────────────────────
        switch (text)
        {
            case "📝 Внести лог":
                await addLogHandler.StartAsync(chatId, cancellationToken);
                return;

            case "⏰ Мои напоминания":
                await remindersHandler.HandleAsync(chatId, cancellationToken);
                return;

            case "➕ Напоминание":
                await addReminderHandler.StartAsync(chatId, cancellationToken);
                return;
        }

        // ── FSM-aware input routing ───────────────────────────────
        DialogueSessionData session = await sessionStore.GetAsync(chatId, cancellationToken);

        if (IsLogState(session.State))
        {
            await addLogHandler.HandleInputAsync(chatId, text, session, cancellationToken);
            return;
        }

        if (IsReminderState(session.State))
        {
            await addReminderHandler.HandleInputAsync(chatId, text, session, cancellationToken);
        }
    }

    // ── Callback query routing ────────────────────────────────────

    private async Task HandleCallbackQueryAsync(JsonElement callbackQuery, CancellationToken cancellationToken)
    {
        string callbackQueryId = callbackQuery.TryGetProperty("id", out JsonElement idEl)
            ? idEl.GetString() ?? string.Empty
            : string.Empty;

        string data = callbackQuery.TryGetProperty("data", out JsonElement dataEl)
            ? dataEl.GetString() ?? string.Empty
            : string.Empty;

        if (!callbackQuery.TryGetProperty("message", out JsonElement msg))
        {
            return;
        }

        if (!TryExtractChatId(msg, out long chatId))
        {
            return;
        }

        int messageId = msg.TryGetProperty("message_id", out JsonElement msgIdEl)
            ? msgIdEl.GetInt32()
            : 0;

        logger.LogDebug("Callback: {Data} from chatId={ChatId}", data, chatId);

        // ── Ecosystem selection (shared between Log and Reminder flows) ──
        if (data.StartsWith("ecosystem:", StringComparison.Ordinal))
        {
            // Route the callback data as text input to the FSM
            DialogueSessionData session = await sessionStore.GetAsync(chatId, cancellationToken);

            if (IsLogState(session.State))
            {
                await addLogHandler.HandleInputAsync(chatId, data, session, cancellationToken);
            }
            else if (IsReminderState(session.State))
            {
                await addReminderHandler.HandleInputAsync(chatId, data, session, cancellationToken);
            }

            return;
        }

        switch (true)
        {
            case true when data.StartsWith("read_notice:", StringComparison.Ordinal):
                string notificationId = data["read_notice:".Length..];
                await markReadHandler.HandleAsync(chatId, messageId, callbackQueryId, notificationId, cancellationToken);
                return;

            case true when data.StartsWith("done_reminder:", StringComparison.Ordinal):
                string reminderId = data["done_reminder:".Length..];
                await completeReminderHandler.HandleAsync(chatId, messageId, callbackQueryId, reminderId, cancellationToken);
                return;

            case true when data == "cancel_flow":
                await cancelFlowHandler.HandleCancelAsync(chatId, messageId, callbackQueryId, cancellationToken);
                return;

            case true when data == "log_skip_notes":
                {
                    DialogueSessionData session = await sessionStore.GetAsync(chatId, cancellationToken);
                    await addLogHandler.HandleInputAsync(chatId, "Пропустить", session, cancellationToken);
                    await cancelFlowHandler.HandleSkipNotesAsync(chatId, callbackQueryId, cancellationToken);
                    return;
                }
        }

        logger.LogWarning("Unhandled callback_data: {Data}", data);
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static bool TryExtractChatId(JsonElement message, out long chatId)
    {
        chatId = 0;
        return message.TryGetProperty("chat", out JsonElement chat)
               && chat.TryGetProperty("id", out JsonElement idEl)
               && idEl.TryGetInt64(out chatId);
    }

    private static bool IsLogState(UserDialogueState state) =>
        state is UserDialogueState.Log_WaitingForEcosystem
              or UserDialogueState.Log_WaitingForPh
              or UserDialogueState.Log_WaitingForKh
              or UserDialogueState.Log_WaitingForNo3
              or UserDialogueState.Log_WaitingForNotes;

    private static bool IsReminderState(UserDialogueState state) =>
        state is UserDialogueState.Reminder_WaitingForEcosystem
              or UserDialogueState.Reminder_WaitingForTaskName
              or UserDialogueState.Reminder_WaitingForInterval;
}
