using Interfaces;
using Microsoft.Extensions.Logging;

namespace Handlers.Commands;

/// <summary>
/// Handles the /start command (with optional deep-link token for account linking).
///
/// Flow:
///   /start             → already linked  → show main menu
///   /start {token}     → link account via IdentityService HMAC call → show main menu
///   account not linked → ask to link via AquaSmart web app
/// </summary>
public sealed class StartCommandHandler(
    IIdentityClient identityClient,
    IFsmSessionStore sessionStore,
    ITelegramResponseService response,
    ILogger<StartCommandHandler> logger)
{
    private const string WelcomeText =
        "🌊 <b>Добро пожаловать в AquaSmart Bot!</b>\n\n" +
        "Я помогу тебе:\n" +
        "• 📝 вносить логи обслуживания аквариума\n" +
        "• ⏰ управлять напоминаниями\n" +
        "• 👁 получать уведомления об отклонениях\n\n" +
        "Используй кнопки меню ниже 👇";

    private const string NotLinkedText =
        "❌ <b>Аккаунт не привязан.</b>\n\n" +
        "Перейди в AquaSmart и в разделе <b>Профиль → Telegram</b> нажми «Привязать бота».";

    private static readonly object _mainMenuKeyboard = new
    {
        keyboard = new[]
        {
            new[] { "📝 Внести лог", "⏰ Мои напоминания" },
            new[] { "➕ Напоминание" }
        },
        resize_keyboard = true,
        persistent = true
    };

    public async Task HandleAsync(long chatId, string? deepLinkPayload, CancellationToken cancellationToken)
    {
        // Reset any stale FSM session on every /start
        await sessionStore.DeleteAsync(chatId, cancellationToken);

        bool isLinked;

        if (!string.IsNullOrWhiteSpace(deepLinkPayload))
        {
            // Deep-link: /start {token}
            isLinked = await identityClient.VerifyLinkTokenAsync(deepLinkPayload, chatId, cancellationToken);
            if (!isLinked)
            {
                logger.LogWarning("Invalid or expired link token for chatId={ChatId}", chatId);
                await response.SendTextAsync(chatId,
                    "⚠️ Токен привязки недействителен или истёк. Попробуй снова из AquaSmart.",
                    cancellationToken);
                return;
            }

            logger.LogInformation("Telegram account linked successfully for chatId={ChatId}", chatId);
        }
        else
        {
            string? token = await identityClient.LoginByChatIdAsync(chatId, cancellationToken);
            isLinked = token is not null;
        }

        if (!isLinked)
        {
            await response.SendTextAsync(chatId, NotLinkedText, cancellationToken);
            return;
        }

        await response.SendWithKeyboardAsync(chatId, WelcomeText, _mainMenuKeyboard, cancellationToken);
    }
}
