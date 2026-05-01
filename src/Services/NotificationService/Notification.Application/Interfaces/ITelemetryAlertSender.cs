using Contracts.Events.TelemetryEvents;
using Contracts.Results;

namespace Notification.Application.Interfaces;

public interface ITelemetryAlertSender
{
    Task<ConsumerResult> SendTelemetryAlertAsync(
        CriticalTelemetryThresholdAlertEvent alertEvent, 
        CancellationToken cancellationToken);
}