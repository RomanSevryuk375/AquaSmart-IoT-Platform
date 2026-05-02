using Contracts.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<bool> ExistsAsync(
        Guid userId, 
        CancellationToken cancellationToken);

    Task<List<UserEntity>> GetAllUsersByIdAsync(
        List<Guid> userIds,
        CancellationToken cancellationToken);
}
