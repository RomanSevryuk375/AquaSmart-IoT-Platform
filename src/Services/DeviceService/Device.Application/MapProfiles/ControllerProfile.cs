namespace Device.Application.MapProfiles;

public sealed class ControllerProfile : Profile
{
    public ControllerProfile()
    {
        CreateMap<Controller, ControllerResponseDto>();
    }
}
