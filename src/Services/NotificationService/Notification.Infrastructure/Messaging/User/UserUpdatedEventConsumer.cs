using AutoMapper;
using Contracts.Events.UserEvents;
using MediatR;
using Notification.Application.Features.Users.Commands.SyncUserUpdated;

namespace Notification.Infrastructure.Messaging.User;

internal sealed class UserUpdatedEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<UserUpdatedEvent, SyncUserUpdatedCommand>(sender, mapper);
