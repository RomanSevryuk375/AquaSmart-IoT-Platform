using Control.Domain.Entities;
using Control.Domain.Interfaces;

namespace Control.Infrastructure.Persistence.Repositories;

public sealed class ScheduleRepository(SystemDbContext dbContext) 
    : BaseRepository<ScheduleEntity>(dbContext), IScheduleRepository
{
}
