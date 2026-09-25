using AutoMapper;
using BuildingBlocks.IntegrationEvents.Events.Users;
using Notification.Application.Features.Users.Commands.SyncTelegramAccountLinked;
using Notification.Application.Features.Users.Commands.SyncUserCreated;
using Notification.Application.Features.Users.Commands.SyncUserUpdated;

namespace Notification.Application.MapProfiles;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCreatedEvent, SyncUserCreatedCommand>();

        CreateMap<UserUpdatedEvent, SyncUserUpdatedCommand>();

        CreateMap<SyncUserCreatedCommand, SyncUserUpdatedCommand>();

        CreateMap<TelegramAccountLinkedEvent, TelegramAccountLinkedCommand>();
    }
}
