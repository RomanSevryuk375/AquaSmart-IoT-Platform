using Contracts.Events.TelemetryEvents;
using Device.Application.Interfaces;
using MassTransit;

namespace Device.Application.Features.Telemetry.Command.TransmitTelemetry;

internal class TransmitTelemetryHandler(
    ISensorRepository sensorRepository,
    IControllerRepository controllerRepository,
    IPublishEndpoint publishEndpoint,
    IMyHasher myHasher,
    IValidator<TransmitTelemetryCommand> batchValidator)
    : IRequestHandler<TransmitTelemetryCommand, Result<TelemetryTransmitedResponse>>
{
    public async Task<Result<TelemetryTransmitedResponse>> Handle(
        TransmitTelemetryCommand request,
        CancellationToken cancellationToken)
    {
        FluentValidation.Results.ValidationResult validationResult = batchValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result<TelemetryTransmitedResponse>.Success(new TelemetryTransmitedResponse
            {
                AcceptedCount = 0,
                SkippedCount = request.Items?.Count ?? 0,
                ValidationErrors = validationResult.Errors.Select(e =>
                    $"{e.PropertyName}: {e.ErrorMessage}").ToList()
            });
        }

        Controller? existingController = await controllerRepository.GetByMacAddressAsync(
            request.MacAddress, cancellationToken);
        if (existingController is null)
        {
            return Result<TelemetryTransmitedResponse>.Failure(Error.NotFound<Controller>(
                    $"{nameof(Controller)} {request.MacAddress} not found"));
        }

        if (!myHasher.Verify(request.DeviceToken, existingController.DeviceTokenHash))
        {
            return Result<TelemetryTransmitedResponse>.Failure(Error.Conflict(
                "Access.Denied", "You are not the owner of this controller"));
        }

        IReadOnlyList<Sensor> existingSensors = await sensorRepository.GetAllSensorsAsync(
            existingController.Id, cancellationToken);

        var response = new TelemetryTransmitedResponse();
        var batchItemsForEvent = new List<TelemetryBatchEventItem>();

        foreach (TelemetryItem item in request.Items)
        {
            bool isOwnSensor = existingSensors.Any(s => s.Id == item.SensorId);

            if (!isOwnSensor)
            {
                response.ValidationErrors.Add(
                    $"Sensor {item.SensorId} does not belong to controller {request.MacAddress}.");
                response.SkippedCount++;
                continue;
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

        if (batchItemsForEvent.Count > 0)
        {
            await publishEndpoint.Publish(new TelemetryBatchEvent
            {
                ControllerId = existingController.Id,
                Items = batchItemsForEvent
            }, cancellationToken);
        }

        return Result<TelemetryTransmitedResponse>.Success(response);
    }
}
