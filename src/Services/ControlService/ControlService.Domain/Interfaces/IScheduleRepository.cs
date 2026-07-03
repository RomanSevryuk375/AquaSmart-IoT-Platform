using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IScheduleRepository : IRepository<Schedule>
{
    public Task<IReadOnlyList<Schedule>> GetActiveSchedules(
        CancellationToken cancellationToken = default);
}
