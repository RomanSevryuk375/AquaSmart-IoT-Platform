using Control.Domain.Entities;
using Control.Domain.Interfaces;

namespace Control.Infrastructure.Repositories;

public sealed class VacationModeRepository(ControlDbContext dbContext)
    : BaseRepository<VacationModeEntity>(dbContext), IVacationModeRepository
{ 

}
