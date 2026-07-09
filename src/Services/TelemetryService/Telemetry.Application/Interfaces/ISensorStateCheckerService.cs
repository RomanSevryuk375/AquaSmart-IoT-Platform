using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface ISensorStateCheckerService
{
    public Task<Result> CheckStateAndNotify(
        CancellationToken cancellationToken);
}
