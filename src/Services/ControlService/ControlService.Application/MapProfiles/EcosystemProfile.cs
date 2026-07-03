using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Control.Application.Features.Ecosystem.Queries;
using Control.Domain.Entities;

namespace Control.Application.MapProfiles;

public sealed class EcosystemProfile : Profile
{
    public EcosystemProfile()
    {
        CreateMap<EcosystemEntity, EcosystemDto>();

        CreateMap<EcosystemEntity, EcosystemCreatedEvent>()
            .ForMember(desc => desc.EcosystemId,
            opt => opt.MapFrom(src => src.Id));

        CreateMap<EcosystemEntity, EcosystemUdatedEvent>()
           .ForMember(desc => desc.EcosystemId,
           opt => opt.MapFrom(src => src.Id));
    }
}
