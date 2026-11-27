using BuildingBlocks.Domain.Results;
using Device.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Device.Application.Features.Mqtt;

public sealed class ProcessDeviceStatusHandler(
    IControllerRepository controllerRepository,
    ILogger<ProcessDeviceStatusHandler> logger)
    : IRequestHandler<ProcessDeviceStatusCommand, Result>
{
    public async Task<Result> Handle(
        ProcessDeviceStatusCommand request,
        CancellationToken cancellationToken)
    {
        Result<MacAddress> macResult = MacAddress.Create(request.TopicMac);
        if (macResult.IsFailure)
        {
            logger.LogWarning("MQTT status ignored: invalid topic MAC {TopicMac}.", request.TopicMac);
            return Result.Success();
        }

        Controller? controller = await controllerRepository.GetByMacAddressAsync(
            macResult.Value.Value, cancellationToken);
        if (controller is null)
        {
            logger.LogWarning("MQTT status ignored: unknown controller for MAC {MacAddress}.", macResult.Value.Value);
            return Result.Success();
        }

        string normalizedStatus = request.Status.Trim().ToLowerInvariant();
        if (string.Equals(normalizedStatus, "online", StringComparison.Ordinal))
        {
            controller.RecordMqttOnline(
                request.FirmwareVersion,
                request.IpAddress,
                request.Rssi,
                request.UptimeSec);

            return Result.Success();
        }

        if (string.Equals(normalizedStatus, "offline", StringComparison.Ordinal))
        {
            controller.MarkOfflineFromLwt();
            return Result.Success();
        }

        logger.LogWarning(
            "MQTT status ignored: unknown status '{Status}' for Controller {ControllerId}.",
            request.Status,
            controller.Id);

        return Result.Success();
    }
}
