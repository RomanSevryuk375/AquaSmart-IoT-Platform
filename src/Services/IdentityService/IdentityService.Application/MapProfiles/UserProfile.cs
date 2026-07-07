using AutoMapper;
using Contracts.Events.UserEvents;
using IdentityService.Domain.Events;

namespace IdentityService.Application.MapProfiles;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCreatedDomainEvent, UserCreatedEvent>();

        CreateMap<UserUpdatedDomainEvent, UserUpdatedEvent>();
    }
}
