using AutoMapper;
using Contracts.Events.ControllerEvents;
using MediatR;
using Notification.Application.Features.Alerts.Commands.SendControllerOfflineAlert;

namespace Notification.Infrastructure.Messaging.Alert;

internal sealed class ControllerNotOnlineEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<ControllerNotOnlineEvent, SendControllerOfflineAlertCommand>(sender, mapper);
