using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class ReminderRepository(NotificationDbContext dbContext)
    : BaseRepository<Reminder>(dbContext), IReminderRepository
{
    public async Task<IReadOnlyList<Reminder>> GetPendingRemindersAsync(
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        return await Context.Reminders
            .AsNoTracking()
            .Where(x => x.NextDueAt <= now)
            .ToListAsync(cancellationToken);
    }
}
