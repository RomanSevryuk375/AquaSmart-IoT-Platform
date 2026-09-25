using AutoMapper;
using BuildingBlocks.IntegrationEvents.Events.Users;
using MediatR;
using Notification.Application.Features.Users.Commands.SyncTelegramAccountLinked;

namespace Notification.Infrastructure.Messaging.User;

internal sealed class TelegramAccountLinkedEventConsumer(ISender sender, IMapper mapper)
    : MediatRIntegrationEventConsumer<TelegramAccountLinkedEvent, TelegramAccountLinkedCommand>(sender, mapper);
