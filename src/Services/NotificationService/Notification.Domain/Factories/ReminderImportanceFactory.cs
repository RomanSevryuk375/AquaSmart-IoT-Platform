using Contracts.Enums;

namespace Notification.Domain.Factories;

public static class ReminderImportanceFactory
{
    private const int InfoThreshold = 24;
    private const int WarningThreshold = 0;
    private const int CriticalThreshold = -24;

    public static NotificationLevel Evaluate(DateTime nextDueAt)
    {
        DateTime now = DateTime.UtcNow;
        TimeSpan timeDiff = nextDueAt - now;

        return timeDiff.TotalHours switch
        {
            <= CriticalThreshold => NotificationLevel.Critical,
            <= WarningThreshold => NotificationLevel.Warning,
            < InfoThreshold => NotificationLevel.Info,

            _ => NotificationLevel.Info
        };
    }
}
