namespace Device.Application.Interfaces;

public interface IControllerOfflineCheckerService
{
    public Task<Result> CheckAndDisableControllerAsync(
        CancellationToken cancellationToken = default);
}
