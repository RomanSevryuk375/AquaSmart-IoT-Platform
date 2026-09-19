using BuildingBlocks.Domain.Abstractions;
using Telemetry.Domain.Entities;

namespace Telemetry.Domain.Interfaces;

public interface IEcosystemRepository : IRepository<Ecosystem>
{
    public Task<Ecosystem?> GetByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default);

    public Task<bool> UserOwnsEcosystemAsync(
        Guid ecosystemId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
