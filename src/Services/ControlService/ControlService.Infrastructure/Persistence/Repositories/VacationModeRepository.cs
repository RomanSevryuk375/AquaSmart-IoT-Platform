using Control.Domain.Entities;
using Control.Domain.Interfaces;

namespace Control.Infrastructure.Persistence.Repositories;

public sealed class VacationModeRepository(SystemDbContext dbContext)
    : BaseRepository<VacationMode>(dbContext), IVacationModeRepository
{

}
