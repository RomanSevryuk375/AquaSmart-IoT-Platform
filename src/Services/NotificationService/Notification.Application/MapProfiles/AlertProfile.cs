using AutoMapper;
using Contracts.Events.ControllerEvents;
using Contracts.Events.SensorEvents;
using Contracts.Events.TelemetryEvents;
using Contracts.Events.UserEvents;
using Notification.Application.Features.Alerts.Commands.SendControllerOfflineAlert;
using Notification.Application.Features.Alerts.Commands.SendSensorNoDataAlert;
using Notification.Application.Features.Alerts.Commands.SendSubscriptionAlert;
using Notification.Application.Features.Alerts.Commands.SendTelemetryAlert;

namespace Notification.Application.MapProfiles;

public sealed class AlertProfile : Profile
{
    public AlertProfile()
    {
        CreateMap<ControllerNotOnlineEvent, SendControllerOfflineAlertCommand>();

        CreateMap<SensorNoDataAlertEvent, SendSensorNoDataAlertCommand>();

        CreateMap<CriticalTelemetryThresholdAlertEvent, SendTelemetryAlertCommand>();

        CreateMap<SubscriptionDowngradedEvent, SendSubscriptionAlertCommand>();
    }
}
