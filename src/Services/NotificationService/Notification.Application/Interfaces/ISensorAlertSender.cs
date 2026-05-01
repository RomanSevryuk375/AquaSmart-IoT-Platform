using Contracts.Events.SensorEvents;
using Contracts.Results;

namespace Notification.Application.Interfaces;

public interface ISensorAlertSender
{
    Task<ConsumerResult> SendSensorNoDataAlertAsync(
        SensorNoDataAlertEvent alertEvent, 
        CancellationToken cancellationToken);
}