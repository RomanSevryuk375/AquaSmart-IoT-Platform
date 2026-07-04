using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence.Repositories;

public sealed class ScheduleRepository(ControlDbContext dbContext)
    : BaseRepository<Schedule>(dbContext), IScheduleRepository
{
    public async Task<IReadOnlyList<Schedule>> GetActiveSchedules(CancellationToken cancellationToken)
    {
        return await Context.Schedules
            .Where(x => x.IsEnabled)
            .ToListAsync(cancellationToken);
    }
}
