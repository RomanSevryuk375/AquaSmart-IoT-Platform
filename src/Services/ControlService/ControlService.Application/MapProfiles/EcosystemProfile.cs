using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Control.Application.Features.Ecosystems.Queries;
using Control.Domain.Entities;
using Control.Domain.Events;

namespace Control.Application.MapProfiles;

public sealed class EcosystemProfile : Profile
{
    public EcosystemProfile()
    {
        CreateMap<Ecosystem, EcosystemDto>();

        CreateMap<Ecosystem, EcosystemCreatedEvent>()
            .ForMember(desc => desc.EcosystemId,
            opt => opt.MapFrom(src => src.Id));

        CreateMap<Ecosystem, EcosystemUpdatedEvent>()
           .ForMember(desc => desc.EcosystemId,
           opt => opt.MapFrom(src => src.Id));

        CreateMap<EcosystemCreatedDomainEvent, EcosystemCreatedEvent>();

        CreateMap<EcosystemUpdatedDomainEvent, EcosystemUpdatedEvent>();

        CreateMap<EcosystemDeletedDomainEvent, EcosystemDeletedEvent>();
    }
}
