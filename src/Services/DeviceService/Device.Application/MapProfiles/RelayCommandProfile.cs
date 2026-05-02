using AutoMapper;
using Device.Application.DTOs.RelayCommands;
using Device.Domain.Entities;

namespace Device.Application.MapProfiles;

public sealed class RelayCommandProfile : Profile
{
    public RelayCommandProfile()
    {
        CreateMap<RelayCommandsQueueEntity, RelayCommandResponseDto>();
    }
}
