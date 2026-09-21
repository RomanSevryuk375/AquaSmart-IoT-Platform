using BuildingBlocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class ReminderRepository(NotificationDbContext dbContext)
    : BaseRepository<NotificationDbContext, Reminder>(dbContext), IReminderRepository
{
    public async Task<IReadOnlyList<Reminder>> GetPendingRemindersAsync(
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        return await Context.Reminders
            .Where(x => x.NextDueAt <= now)
            .ToListAsync(cancellationToken);
    }
}
