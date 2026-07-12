using AutoMapper;
using Contracts.Events.UserEvents;
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
    }
}
