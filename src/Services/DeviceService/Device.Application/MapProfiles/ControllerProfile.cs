using AutoMapper;
using Device.Application.DTOs.Controller;
using Device.Domain.Entities;

namespace Device.Application.MapProfiles;

public class ControllerProfile : Profile
{
    public ControllerProfile()
    {
        CreateMap<ControllerEntity, ControllerResponseDto>();
    }
}
