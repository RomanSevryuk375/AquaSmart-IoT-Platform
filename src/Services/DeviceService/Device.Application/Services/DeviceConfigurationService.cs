using AutoMapper;
using Contracts.Results;
using Device.Application.DTOs.Configurations;
using Device.Application.Interfaces;
using Device.Domain.Interfaces;

namespace Device.Application.Services;

public sealed class DeviceConfigurationService(
    IControllerRepository controllerRepository,
    ISensorRepository sensorRepository,
    IRelayRepository relayRepository,
    IMyHasher myHasher,
    IMapper mapper) : IDeviceConfigurationService
{
    public async Task<Result<ConfigResponseDto>> GetControllerConfigAsync(
        string macAddress,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        var controller = await controllerRepository
            .GetByMacAddressAsync(macAddress, cancellationToken);

        if (controller is null)
        {
            return Result<ConfigResponseDto>.Failure(
                Error.NotFound("Controller.NotFound",
                              $"Controller {macAddress} not found."));
        }

        if (!myHasher.Verify(deviceToken, controller.DeviceTokenHash))
        {
            return Result<ConfigResponseDto>.Failure(
                Error.Conflict("Controller.Conflict",
                              $"Controller have incorrect device token {deviceToken}."));
        }

        var relays = await relayRepository
            .GetAllByControllerId(controller.Id, cancellationToken);

        var sensors = await sensorRepository
            .GetAllSensorsAsync(controller.Id, cancellationToken);

        return Result<ConfigResponseDto>.Success(
            new ConfigResponseDto
            {
                SendIntervalMs = 5000,
                MaxBatchSize = 50,
                Relays = mapper.Map<IReadOnlyList<RelayConfigDto>>(relays),
                Sensors = mapper.Map<IReadOnlyList<SensorConfigDto>>(sensors),
            });
    }
}
