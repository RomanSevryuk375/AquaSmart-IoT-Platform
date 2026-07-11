using AutoMapper;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Features.AutomationRules.Commands.CreateRule;
using Control.Application.Features.AutomationRules.Queries;
using Control.Domain.Entities;

namespace Control.Application.MapProfiles;

public sealed class AutomationRuleProfile : Profile
{
    public AutomationRuleProfile()
    {
        CreateMap<CreateRuleRequestDto, CreateRuleCommand>();

        CreateMap<AutomationRule, AutomationRuleDto>();

        CreateMap<AutomationRule, AutomationRuleDto>()
            .ForMember(dest => dest.Conditions,
            opt => opt.MapFrom(src => src.Conditions));
    }
}
