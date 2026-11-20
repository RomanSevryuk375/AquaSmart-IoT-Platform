using Mobile.Services.Dto.Notification;

namespace Mobile.Services.Interfaces;

public interface IReminderService
{
    Task<List<ReminderDto>> GetRemindersAsync(Guid? ecosystemId = null, CancellationToken cancellationToken = default);
    Task<bool> CreateReminderAsync(CreateReminderRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> CompleteReminderAsync(Guid reminderId, CancellationToken cancellationToken = default);
    Task<bool> DeleteReminderAsync(Guid reminderId, CancellationToken cancellationToken = default);
}
