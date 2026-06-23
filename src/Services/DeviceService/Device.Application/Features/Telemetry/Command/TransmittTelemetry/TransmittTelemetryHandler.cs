using Contracts.Events.TelemetryEvents;
using MassTransit;

namespace Device.Application.Features.Telemetry.Command.TransmittTelemetry;

internal sealed class TransmittTelemetryHandler(
    ISensorRepository sensorRepository,
    IControllerRepository controllerRepository,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<TransmitTelemetryCommand, Result<TelemetryTransmittedResponse>>
{
    public async Task<Result<TelemetryTransmittedResponse>> Handle(
        TransmitTelemetryCommand request,
        CancellationToken cancellationToken)
    {
        Controller? existingController = await controllerRepository.GetByMacAddressAsync(
            request.MacAddress, cancellationToken);

        var controllerSensorIds = (await sensorRepository.GetAllSensorsAsync(
            existingController!.Id, cancellationToken)).Select(x => x.Id).ToHashSet();

        var response = new TelemetryTransmittedResponse();
        var batchItemsForEvent = new List<TelemetryBatchEventItem>();

        foreach (TelemetryItem item in request.Items)
        {
            TransmittItem(item, controllerSensorIds, response, request, batchItemsForEvent);
        }

        if (batchItemsForEvent.Count > 0)
        {
            await publishEndpoint.Publish(new TelemetryBatchEvent
            {
                ControllerId = existingController.Id,
                Items = batchItemsForEvent
            }, cancellationToken);
        }

        return Result<TelemetryTransmittedResponse>.Success(response);
    }

    private static void TransmittItem(
        TelemetryItem item,
        HashSet<Guid> controllerSensorIds,
        TelemetryTransmittedResponse response,
        TransmitTelemetryCommand request,
        List<TelemetryBatchEventItem> batchItemsForEvent)
    {
        if (controllerSensorIds.Contains(item.SensorId))
        {
            response.ValidationErrors.Add(
                $"Sensor {item.SensorId} does not belong to controller {request.MacAddress}.");
            response.SkippedCount++;
            return;
        }

        batchItemsForEvent.Add(new TelemetryBatchEventItem
        {
            SensorId = item.SensorId,
            Value = item.Value,
            ExternalMessageId = item.ExternalMessageId,
            RecordedAt = item.RecordedAt,
        });

        response.AcceptedCount++;
    }
}
