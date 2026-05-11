using Contracts.Results;

namespace Device.Application.Interfaces;

public interface IControllerOfflineCheckerService
{
    Task<Result> CheckAndDisableController(
        CancellationToken cancellationToken);
}
