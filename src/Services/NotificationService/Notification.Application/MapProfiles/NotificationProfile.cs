using AutoMapper;
using Notification.Application.DTOs.Notification;

namespace Notification.Application.MapProfiles;

public sealed class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Domain.Entities.Notification, NotificationResponseDto>();
    }
}
