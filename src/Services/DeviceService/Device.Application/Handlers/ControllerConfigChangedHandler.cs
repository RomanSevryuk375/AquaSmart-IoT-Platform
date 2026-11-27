using Device.Application.Interfaces;
using Device.Domain.Events.ControllerEvents;
using Microsoft.Extensions.Logging;

namespace Device.Application.Handlers;

public sealed class ControllerConfigChangedHandler(
    IMqttCommandPublisher mqttCommandPublisher,
    ILogger<ControllerConfigChangedHandler> logger) : INotificationHandler<ControllerConfigChangedDomainEvent>
{
    public async Task Handle(
        ControllerConfigChangedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var payload = new ConfigNotifyMqttPayload
        {
            Reason = notification.Reason,
            Version = notification.ConfigVersion,
            Timestamp = notification.OccurredOn,
        };

        bool published = await mqttCommandPublisher.PublishConfigNotifyAsync(
            notification.MacAddress, payload, cancellationToken);

        if (!published)
        {
            logger.LogWarning(
                "Failed to publish MQTT config/notify v{Version} for Controller {ControllerId} (MAC: {MacAddress}).",
                notification.ConfigVersion,
                notification.ControllerId,
                notification.MacAddress);
        }
    }
}
