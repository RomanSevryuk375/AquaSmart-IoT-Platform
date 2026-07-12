using AutoMapper;
using Contracts.Events.UserEvents;
using MediatR;
using Notification.Application.Features.Alerts.Commands.SendSubscriptionAlert;

namespace Notification.Infrastructure.Messaging.User;

internal sealed class SubscriptionDowngradedEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<SubscriptionDowngradedEvent, SendSubscriptionAlertCommand>(sender, mapper);
