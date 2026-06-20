using Device.Application.DTOs.Configurations;
using Device.Application.Extesions;
using Device.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Device.Application.Services;

public sealed class DeviceConfigurationService(
    IControllerRepository controllerRepository,
    ISensorRepository sensorRepository,
    IRelayRepository relayRepository,
    IMapper mapper,
    IDeviceSecurityService securityService,
    IOptions<DeviceSettings> deviceOptions) : IDeviceConfigurationService
{
    public async Task<Result<ConfigResponseDto>> GetControllerConfigAsync(
        string macAddress,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        Controller? controller = await controllerRepository
            .GetByMacAddressAsync(macAddress, cancellationToken);

        if (controller is null)
        {
            return Result<ConfigResponseDto>
                .Failure(Error.NotFound(
                    "Controller.NotFound",
                    "Controller not found"));
        }

        Result ownership = await securityService.EnsureDeviceAccessAsync(
            controller.Id, deviceToken, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<ConfigResponseDto>
                .Failure(ownership.Error);
        }

        IReadOnlyList<Relay> relays = await relayRepository
            .GetAllByControllerId(controller.Id, cancellationToken);

        IReadOnlyList<Sensor> sensors = await sensorRepository
            .GetAllSensorsAsync(controller.Id, cancellationToken);

        return Result<ConfigResponseDto>.Success(
            new ConfigResponseDto
            {
                SendIntervalMs = deviceOptions.Value.DefaultSendIntervalMs,
                MaxBatchSize = deviceOptions.Value.MaxConfigBatchSize,
                Relays = mapper.Map<IReadOnlyList<RelayConfigDto>>(relays),
                Sensors = mapper.Map<IReadOnlyList<SensorConfigDto>>(sensors),
            });
    }
}
