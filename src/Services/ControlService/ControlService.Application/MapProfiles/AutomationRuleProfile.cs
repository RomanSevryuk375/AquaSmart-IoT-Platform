using AutoMapper;
using Control.Application.Features.AutomationRule.Queries;
using Control.Domain.Entities;

namespace Control.Application.MapProfiles;

public sealed class AutomationRuleProfile : Profile
{
    public AutomationRuleProfile()
    {
        CreateMap<AutomationRule, AutomationRuleDto>();

        CreateMap<AutomationRule, AutomationRuleDto>()
            .ForMember(dest => dest.Conditions,
            opt => opt.MapFrom(src => src.Conditions));
    }
}
