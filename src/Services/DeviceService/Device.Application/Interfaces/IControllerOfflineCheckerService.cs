namespace Device.Application.Interfaces;

public interface IControllerOfflineCheckerService
{
    public Task<Result<int>> CheckAndDisableControllerAsync(
        CancellationToken cancellationToken = default);
}
