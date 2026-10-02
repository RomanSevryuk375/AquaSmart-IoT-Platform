using DTOs;
using Interfaces;

namespace Handlers.Commands;

/// <summary>
/// Handles the /reminders command and "⏰ Мои напоминания" button press.
/// Fetches all reminders, sorts by urgency and formats a visual summary.
/// </summary>
public sealed class ShowRemindersHandler(
    INotificationApiClient notificationClient,
    ITelegramResponseService response)
{
    public async Task HandleAsync(long chatId, CancellationToken cancellationToken)
    {
        IReadOnlyList<ReminderDto> reminders =
            await notificationClient.GetAllRemindersAsync(chatId, cancellationToken);

        if (reminders.Count == 0)
        {
            await response.SendTextAsync(
                chatId,
                "📭 <b>Напоминания не найдены.</b>\n\nДобавь первое, нажав «➕ Напоминание».",
                cancellationToken);
            return;
        }

        DateTime now = DateTime.UtcNow;

        IOrderedEnumerable<ReminderDto> sorted = reminders
            .OrderBy(r => r.NextDueAt);

        var lines = new System.Text.StringBuilder();
        lines.AppendLine("⏰ <b>Твои напоминания:</b>\n");

        foreach (ReminderDto r in sorted)
        {
            string emoji = GetUrgencyEmoji(r.NextDueAt, now);
            int daysLeft = (int)(r.NextDueAt.Date - now.Date).TotalDays;
            string daysText = daysLeft switch
            {
                < 0 => $"просрочено на {-daysLeft} дн.",
                0 => "сегодня",
                1 => "завтра",
                _ => $"через {daysLeft} дн."
            };

            lines.AppendLine(
                $"{emoji} <b>{r.TaskName}</b>\n" +
                $"   📅 {r.NextDueAt:dd.MM.yyyy} ({daysText}) · 🔄 каждые {r.IntervalDays} дн.\n");
        }

        var rows = sorted
            .Select(r => new[]
            {
                new { text = $"✅ {r.TaskName}", callback_data = $"done_reminder:{r.Id}" }
            })
            .ToArray();

        var keyboard = new { inline_keyboard = rows };

        await response.SendWithKeyboardAsync(chatId, lines.ToString(), keyboard, cancellationToken);
    }

    private static string GetUrgencyEmoji(DateTime nextDueAt, DateTime now)
    {
        int daysLeft = (int)(nextDueAt.Date - now.Date).TotalDays;
        return daysLeft switch
        {
            < 0 => "🔴",
            0 => "🟡",
            _ => "🟢"
        };
    }
}
