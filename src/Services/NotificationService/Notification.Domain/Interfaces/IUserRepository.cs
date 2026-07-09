using Contracts.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    public Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    public Task<List<User>> GetAllUsersByIdAsync(
        List<Guid> userIds,
        CancellationToken cancellationToken = default);
}
