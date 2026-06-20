namespace Device.Application.Interfaces;

public interface IDeviceSecurityService
{
    public Task<Result> EnsureDeviceAccessAsync(
        Guid controllerId,
        string deviceToken,
        CancellationToken cancellationToken = default);

    public Task<Result> EnsureUserOwnsControllerAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default);
}
