namespace Device.Application.Interfaces;

public interface IDeviceSecurityService
{
    Task<Result> EnsureDeviceAccessAsync(
        Guid controllerId, 
        string deviceToken, 
        CancellationToken cancellationToken);

    Task<Result> EnsureUserOwnsControllerAsync(
        Guid controllerId, 
        CancellationToken cancellationToken);
}