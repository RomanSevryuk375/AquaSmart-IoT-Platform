using Contracts.Results;
using Notification.Application.DTOs.Reminder;

namespace Notification.Application.Interfaces;

public interface IReminderService
{
    Task<Result<Guid>> AddReminderAsync(
        ReminderRequestDto request, 
        CancellationToken cancellationToken);

    Task<Result> DeleteReminderAsync(
        Guid reminderId,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ReminderResponseDto>>> GetAllRemindersAsync(
        ReminderFilterDto filter, 
        int? skip, 
        int? take, 
        CancellationToken cancellationToken);

    Task<Result<ReminderResponseDto>> GetReminderByIdAsync(
        Guid reminderId, 
        CancellationToken cancellationToken);

    Task<Result> ReminderCompleteTaskAsync(
        Guid reminderId, 
        CancellationToken cancellationToken);

    Task<Result> UpdateReminderAsync(
        Guid reminderId, 
        ReminderUpdateRequestDto request, 
        CancellationToken cancellationToken);
}