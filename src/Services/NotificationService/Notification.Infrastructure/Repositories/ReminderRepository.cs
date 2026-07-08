using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Repositories;

public sealed class ReminderRepository(SystemDbContext dbContext)
    : BaseRepository<Reminder>(dbContext), IReminderRepository
{
    public async Task<IReadOnlyList<Reminder>> GetPendingRemindersAsync(
        DateTime now, 
        CancellationToken cancellationToken)
    {
        return await Context.Reminders
            .AsNoTracking()
            .Where(x => x.NextDueAt <= now)
            .ToListAsync(cancellationToken);
    }
}
