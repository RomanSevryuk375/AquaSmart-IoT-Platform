using AutoMapper;
using Contracts.Enums;
using Contracts.Events.SensorEvents;
using Contracts.Exceptions;
using Device.Application.DTOs.Sensor;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.Domain.SpecificationParams;
using Device.Domain.Specifications;
using FluentValidation;
using MassTransit;

namespace Device.Application.Services;

public sealed class SensorService(
    ISensorRepository sensorRepository,
    IControllerRepository controllerRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IMapper mapper,
    IValidator<SensorRequestDto> createValidator,
    IValidator<SensorUpdateRequestDto> updateValidator) : ISensorService
{
    public async Task<Guid> AddSensorAsync(
        SensorRequestDto request,
        CancellationToken cancellationToken)
    {
        createValidator.ValidateAndThrow(request);

        var existingController = await controllerRepository
            .GetByIdAsync(request.ControllerId, cancellationToken)
            ?? throw new NotFoundException($"Controller {request.ControllerId} not found");

        var (sensor, errors) = SensorEntity.Create(
            request.ControllerId,
            request.Name,
            request.ConnectionProtocol,
            request.ConnectionAddress,
            request.Type,
            request.Unit);

        if (sensor is null)
        {
            throw new DomainValidationException(
                $"Failed to create {nameof(SensorEntity)}: {string.Join(", ", errors)}");
        }

        var result = await sensorRepository.AddAsync(sensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            mapper.Map<SensorCreatedEvent>(sensor), 
            cancellationToken);

        return result;
    }

    public async Task DeleteSensorAsync(
        Guid sensorId,
        CancellationToken cancellationToken)
    {
        await sensorRepository.DeleteAsync(sensorId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new SensorDeletedEvent
        {
            SensorId = sensorId,
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<SensorResponseDto>> GetAllSensorsAsync(
        SensorFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var specification = new SensorFilterSpecification(
            new SensorFilterParams
            {
                ControllerId = filter.ControllerId,
                Type = filter.Type,
                State = filter.State,
            });

        var sensors = await sensorRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        return mapper.Map<IReadOnlyList<SensorResponseDto>>(sensors);
    }

    public async Task<SensorResponseDto> GetSensorByIdAsync(
        Guid sensorId,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository
            .GetByIdAsync(sensorId, cancellationToken)
            ?? throw new NotFoundException($"Sensor {sensorId} not found");

        return mapper.Map<SensorResponseDto>(existingSensor);
    }

    public async Task SetSensorStateAsync(
        Guid sensorId,
        SensorStateEnum state,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository
            .GetByIdAsync(sensorId, cancellationToken)
            ?? throw new NotFoundException($"Sensor {sensorId} not found");

        existingSensor.SetState(state);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            mapper.Map<SensorStateChangedCommand>(existingSensor), 
            cancellationToken);
    }

    public async Task UpdateSensorAsync(
        Guid sensorId,
        SensorUpdateRequestDto updateRequestDto,
        CancellationToken cancellationToken)
    {
        updateValidator.ValidateAndThrow(updateRequestDto);

        var existingSensor = await sensorRepository
            .GetByIdAsync(sensorId, cancellationToken)
            ?? throw new NotFoundException($"Sensor {sensorId} not found");

        var errors = existingSensor.Update(
            updateRequestDto.ConnectionProtocol,
            updateRequestDto.ConnectionAddress,
            updateRequestDto.ControllerId,
            updateRequestDto.Type,
            updateRequestDto.Unit);

        if (errors is not null && errors.Count > 0)
        {
            throw new DomainValidationException(
                $"Update failed: {string.Join(", ", errors)}");
        }

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            mapper.Map<SensorUpdatedEvent>(existingSensor), 
            cancellationToken);
    }
}
