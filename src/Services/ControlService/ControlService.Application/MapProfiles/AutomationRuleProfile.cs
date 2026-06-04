using AutoMapper;
using Control.Application.CQRS.AutomationRule.Queries;
using Control.Domain.Entities;

namespace Control.Application.MapProfiles;

public sealed class AutomationRuleProfile : Profile
{
    public AutomationRuleProfile()
    {
        CreateMap<AutomationRuleEntity, AutomationRuleDto>();

        CreateMap<AutomationRuleEntity, AutomationRuleDto>()
            .ForMember(dest => dest.Conditions, 
            opt => opt.MapFrom(src => src.Conditions));
    }
}
