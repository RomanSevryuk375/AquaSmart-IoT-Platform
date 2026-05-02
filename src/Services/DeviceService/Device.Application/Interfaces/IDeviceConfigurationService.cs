using Contracts.Results;
using Device.Application.DTOs.Configurations;

namespace Device.Application.Interfaces;

public interface IDeviceConfigurationService
{
    Task<Result<ConfigResponseDto>> GetControllerConfigAsync(
        string macAddress, 
        string deviceToken, 
        CancellationToken cancellationToken);
}