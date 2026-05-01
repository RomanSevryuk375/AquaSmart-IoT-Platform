using AutoMapper;
using Notification.Application.DTOs.Notification;
using Notification.Domain.Entities;

namespace Notification.Application.MapProfiles;

public sealed class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<NotificationEntity, NotificationResponseDto>();
    }
}
