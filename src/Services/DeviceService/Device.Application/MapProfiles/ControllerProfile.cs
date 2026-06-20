using Contracts.Events.ControllerEvents;
using Device.Domain.Events.ControllerEvents;

namespace Device.Application.MapProfiles;

public sealed class ControllerProfile : Profile
{
    public ControllerProfile()
    {
        CreateMap<ControllerNotOnlineDomainEvent, ControllerNotOnlineEvent>();
    }
}
