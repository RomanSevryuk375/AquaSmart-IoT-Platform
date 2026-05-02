using Contracts.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface IReminderRepository : IRepository<ReminderEntity>
{
    Task<IReadOnlyList<ReminderEntity>> GetPendingRemindersAsync(
        DateTime now, 
        CancellationToken cancellationToken);
}
