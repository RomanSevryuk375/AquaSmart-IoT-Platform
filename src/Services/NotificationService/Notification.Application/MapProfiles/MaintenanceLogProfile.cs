using AutoMapper;
using Notification.Application.DTOs.MaintenanceLog;
using Notification.Domain.Entities;

namespace Notification.Application.MapProfiles;

public class MaintenanceLogProfile : Profile
{
    public MaintenanceLogProfile()
    {
        CreateMap<MaintenanceLogEntity, MaintenanceLogResponseDto>();
    }
}
