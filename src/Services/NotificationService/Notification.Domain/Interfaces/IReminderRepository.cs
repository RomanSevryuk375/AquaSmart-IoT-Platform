using Contracts.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface IReminderRepository : IRepository<Reminder>
{
    public Task<IReadOnlyList<Reminder>> GetPendingRemindersAsync(
        DateTime now,
        CancellationToken cancellationToken = default);
}
