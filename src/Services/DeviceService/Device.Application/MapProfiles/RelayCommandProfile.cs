using Device.Application.DTOs.RelayCommands;

namespace Device.Application.MapProfiles;

public sealed class RelayCommandProfile : Profile
{
    public RelayCommandProfile()
    {
        CreateMap<RelayCommand, RelayCommandResponseDto>();
    }
}
