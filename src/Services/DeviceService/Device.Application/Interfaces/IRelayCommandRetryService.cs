using BuildingBlocks.Domain.Results;

namespace Device.Application.Interfaces;

public interface IRelayCommandRetryService
{
    public Task<Result<int>> ProcessActiveCommandsAsync(CancellationToken cancellationToken = default);

    public Task<Result<int>> RedeliverPendingForControllerAsync(
        string macAddress,
        CancellationToken cancellationToken = default);
}
