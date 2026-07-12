using AutoMapper;
using Contracts.Events.UserEvents;
using MediatR;
using Notification.Application.Features.Users.Commands.SyncUserCreated;

namespace Notification.Infrastructure.Messaging.User;

internal sealed class UserCreatedEventConsumer(ISender sender, IMapper mapper) :
    MediatRIntegrationEventConsumer<UserCreatedEvent, SyncUserCreatedCommand>(sender, mapper);
