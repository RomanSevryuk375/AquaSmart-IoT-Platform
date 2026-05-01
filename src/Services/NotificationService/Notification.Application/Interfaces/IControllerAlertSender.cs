using Contracts.Events.ControllerEvents;
using Contracts.Results;

namespace Notification.Application.Interfaces;

public interface IControllerAlertSender
{
    Task<ConsumerResult> SendControllerNotOnlineAlert(
        ControllerNotOnlineEvent controllerEvent, 
        CancellationToken cancellationToken);
}