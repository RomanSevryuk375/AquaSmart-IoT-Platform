using Contracts.Results;
using Notification.Application.DTOs.Reminder;

namespace Notification.Application.Interfaces;

public interface IReminderService
{
    public Task<Result<Guid>> AddReminderAsync(
        ReminderRequestDto request,
        CancellationToken cancellationToken);

    public Task<Result> DeleteReminderAsync(
        Guid reminderId,
        CancellationToken cancellationToken);

    public Task<Result<ReminderResponseDto>> GetReminderByIdAsync(
        Guid reminderId,
        CancellationToken cancellationToken);

    public Task<Result> ReminderCompleteTaskAsync(
        Guid reminderId,
        CancellationToken cancellationToken);

    public Task<Result> UpdateReminderAsync(
        Guid reminderId,
        ReminderUpdateRequestDto request,
        CancellationToken cancellationToken);
}
