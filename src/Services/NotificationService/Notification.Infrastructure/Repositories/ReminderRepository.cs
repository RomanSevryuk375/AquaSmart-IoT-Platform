using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Repositories;

public sealed class ReminderRepository(NotificationDbContext dbContext)
    : BaseRepository<ReminderEntity>(dbContext), IReminderRepository
{
    public async Task<IReadOnlyList<ReminderEntity>> GetPendingRemindersAsync(
        DateTime now, 
        CancellationToken cancellationToken)
    {
        return await Context.Reminders
            .AsNoTracking()
            .Where(x => x.NextDueAt <= now)
            .ToListAsync(cancellationToken);
    }
}
