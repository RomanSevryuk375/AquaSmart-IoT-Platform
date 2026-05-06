using Control.Domain.Entities;
using Control.Domain.Interfaces;

namespace Control.Infrastructure.Repositories;

public sealed class ScheduleRepository(ControlDbContext dbContext) 
    : BaseRepository<ScheduleEntity>(dbContext), IScheduleRepository
{
}
