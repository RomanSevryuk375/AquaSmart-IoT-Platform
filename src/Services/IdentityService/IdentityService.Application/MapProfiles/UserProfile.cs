using AutoMapper;
using BuildingBlocks.IntegrationEvents.Events.Users;
using IdentityService.Domain.Events;

namespace IdentityService.Application.MapProfiles;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCreatedDomainEvent, UserCreatedEvent>();

        CreateMap<UserUpdatedDomainEvent, UserUpdatedEvent>();

        CreateMap<SubscriptionDowngradedDomainEvent, SubscriptionDowngradedEvent>();
    }
}
