using Contracts.Events.TelemetryEvents;
using Contracts.Exceptions;
using Device.Application.DTOs.Telemetry;
using Device.Application.Interfaces;
using Device.Domain.Interfaces;
using FluentValidation;
using MassTransit;

namespace Device.Application.Services;

public sealed class TelemetryBatchService(
    ISensorRepository sensorRepository,
    IControllerRepository controllerRepository,
    IPublishEndpoint publishEndpoint,
    IMyHasher myHasher,
    IValidator<TelemetryBatchRequest> batchValidator) : ITelemtryBatchService
{
    public async Task<TelemetryResponse> ProcessTelemetryBatchAsync(
        TelemetryBatchRequest request,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        var validationResult = batchValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return new TelemetryResponse
            {
                AcceptedCount = 0,
                SkippedCount = request.Items?.Count ?? 0,
                ValidationErrors = validationResult.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList()
            };
        }

        var existingController = await controllerRepository
            .GetByMacAddressAsync(request.MacAddress, cancellationToken)
            ?? throw new NotFoundException($"Controller {request.MacAddress} not found");

        if (!myHasher.Verify(deviceToken, existingController.DeviceTokenHash))
        {
            throw new InvalidCredentialsException("DeviceToken is not verified.");
        }

        var existingSensors = await sensorRepository
            .GetAllSensorsAsync(existingController.Id, cancellationToken);

        var response = new TelemetryResponse();

        var batchItemsForEvent = new List<TelemetryBatchEventItem>();

        foreach (var item in request.Items)
        {
            var isOwnSensor = existingSensors.Any(s => s.Id == item.SensorId);

            if (!isOwnSensor)
            {
                response.ValidationErrors
                    .Add($"Sensor {item.SensorId} does not belong " +
                         $"to controller {request.MacAddress}.");
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

        return response;
    }
}
