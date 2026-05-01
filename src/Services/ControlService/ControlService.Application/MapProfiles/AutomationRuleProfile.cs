using AutoMapper;
using Control.Application.DTOs.AutomationRule;
using Control.Domain.Entities;

namespace Control.Application.MapProfiles;

public sealed class AutomationRuleProfile : Profile
{
    public AutomationRuleProfile()
    {
        CreateMap<AutomationRuleEntity, AutomationRuleResponseDto>();

        CreateMap<AutomationRuleEntity, AutomationRuleResponseDto>()
            .ForMember(dest => dest.Conditions, 
            opt => opt.MapFrom(src => src.Conditions));
    }
}
