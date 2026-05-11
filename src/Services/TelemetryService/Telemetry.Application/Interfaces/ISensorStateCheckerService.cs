using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface ISensorStateCheckerService
{
    Task<Result> CheckStateAndNotify(
        CancellationToken cancellationToken);
}
