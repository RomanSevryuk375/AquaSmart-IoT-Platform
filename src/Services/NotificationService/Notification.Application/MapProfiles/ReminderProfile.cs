using AutoMapper;
using Notification.Application.DTOs.Reminder;
using Notification.Domain.Entities;

namespace Notification.Application.MapProfiles;

public sealed class ReminderProfile : Profile
{
    public ReminderProfile()
    {
        CreateMap<Reminder, ReminderResponseDto>();
    }
}
