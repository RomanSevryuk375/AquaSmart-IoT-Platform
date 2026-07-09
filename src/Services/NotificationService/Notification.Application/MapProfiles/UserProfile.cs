using AutoMapper;
using Contracts.Events.UserEvents;
using Notification.Application.Features.Users.Commands.SyncUserCreated;
using Notification.Application.Features.Users.Commands.SyncUserUpdated;

namespace Notification.Application.MapProfiles;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCreatedEvent, SyncUserUpdateCommand>();

        CreateMap<UserUpdatedEvent, SyncUserUpdatedCommand>();

        CreateMap<SyncUserUpdateCommand, SyncUserUpdatedCommand>();
    }
}
