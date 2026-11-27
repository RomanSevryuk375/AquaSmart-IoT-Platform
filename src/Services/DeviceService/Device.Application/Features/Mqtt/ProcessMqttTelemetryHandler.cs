using BuildingBlocks.Domain.Results;
using BuildingBlocks.IntegrationEvents.Events.Telemetrys;
using Device.Domain.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Device.Application.Features.Mqtt;

public sealed class ProcessMqttTelemetryHandler(
    IControllerRepository controllerRepository,
    ISensorRepository sensorRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<ProcessMqttTelemetryHandler> logger)
    : IRequestHandler<ProcessMqttTelemetryCommand, Result>
{
    private const int MaxBatchSize = 50;
    private const int MaxExternalMessageIdLength = 100;

    public async Task<Result> Handle(
        ProcessMqttTelemetryCommand request,
        CancellationToken cancellationToken)
    {
        Result<MacAddress> macResult = MacAddress.Create(request.TopicMac);
        if (macResult.IsFailure)
        {
            logger.LogWarning(
                "MQTT telemetry dropped: invalid topic MAC {TopicMac}.",
                request.TopicMac);
            return Result.Success();
        }

        string canonicalMac = macResult.Value.Value;

        if (request.Items.Count == 0 || request.Items.Count > MaxBatchSize)
        {
            logger.LogWarning(
                "MQTT telemetry dropped for MAC {MacAddress}: invalid item count {Count} (allowed 1..{Max}).",
                canonicalMac,
                request.Items.Count,
                MaxBatchSize);
            return Result.Success();
        }

        Controller? controller = await controllerRepository.GetByMacAddressAsync(canonicalMac, cancellationToken);
        if (controller is null)
        {
            logger.LogWarning(
                "MQTT telemetry dropped: unknown controller for MAC {MacAddress}.",
                canonicalMac);
            return Result.Success();
        }

        IReadOnlyList<Sensor> controllerSensors = await sensorRepository.GetAllSensorsAsync(
            controller.Id, cancellationToken);
        HashSet<Guid> ownedSensorIds = controllerSensors.Select(s => s.Id).ToHashSet();

        var validItems = new List<TelemetryBatchEventItem>(request.Items.Count);
        int skippedCount = 0;

        foreach (MqttTelemetryItemDto item in request.Items)
        {
            if (item is null ||
                item.SensorId == Guid.Empty ||
                !ownedSensorIds.Contains(item.SensorId) ||
                string.IsNullOrWhiteSpace(item.ExternalMessageId) ||
                item.ExternalMessageId.Length > MaxExternalMessageIdLength ||
                double.IsNaN(item.Value) ||
                double.IsInfinity(item.Value))
            {
                skippedCount++;
                continue;
            }

            validItems.Add(new TelemetryBatchEventItem
            {
                SensorId = item.SensorId,
                Value = item.Value,
                ExternalMessageId = item.ExternalMessageId.Trim(),
                RecordedAt = item.RecordedAt == default ? DateTime.UtcNow : item.RecordedAt.ToUniversalTime(),
            });
        }

        if (skippedCount > 0)
        {
            logger.LogWarning(
                "MQTT telemetry partial accept for Controller {ControllerId} (MAC: {MacAddress}): accepted {AcceptedCount}, skipped {SkippedCount}.",
                controller.Id,
                canonicalMac,
                validItems.Count,
                skippedCount);
        }

        if (validItems.Count == 0)
        {
            return Result.Success();
        }

        var integrationEvent = new TelemetryBatchReceivedFromDeviceEvent
        {
            ControllerId = controller.Id,
            UserId = controller.UserId,
            MacAddress = controller.MacAddress.Value,
            Items = validItems,
        };

        await publishEndpoint.Publish(integrationEvent, cancellationToken);

        return Result.Success();
    }
}
