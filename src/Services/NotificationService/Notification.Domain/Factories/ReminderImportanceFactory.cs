using Contracts.Enums;

namespace Notification.Domain.Factories;

public static class ReminderImportanceFactory
{
    public static NotificationLevel Evaluate (DateTime nextDueAt)
    {
        var now = DateTime.UtcNow;
        var timeDiff = nextDueAt - now;

        if (timeDiff.Hours < 24)
        {
            return NotificationLevel.Info;
        }

        if (timeDiff.Hours <= 0)
        {
            return NotificationLevel.Warning;
        }

        if (timeDiff.Hours <= -24)
        {
            return NotificationLevel.Critical;
        }

        return NotificationLevel.Info;
    }
}
