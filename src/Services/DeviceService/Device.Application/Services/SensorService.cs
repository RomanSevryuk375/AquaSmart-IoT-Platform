using AutoMapper;
using Contracts.Enums;
using Contracts.Events.SensorEvents;
using Contracts.Results;
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
    IValidator<SensorUpdateRequestDto> updateValidator,
    IUserContext userContext) : ISensorService
{
    public async Task<Result<Guid>> AddSensorAsync(
        SensorRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = createValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "CreateRequest.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var controller = await controllerRepository
            .GetByIdAsync(request.ControllerId, cancellationToken);

        if (controller is null)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {request.ControllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result<Guid>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        var (sensor, errors) = SensorEntity.Create(
            request.ControllerId,
            userContext.UserId,
            request.Name,
            request.ConnectionProtocol,
            request.ConnectionAddress,
            request.Type,
            request.Unit);

        if (sensor is null)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "Sensor.Invalid",
                    $"Failed to create {nameof(SensorEntity)}: {string.Join(", ", errors!)}"));
        }

        var result = await sensorRepository.AddAsync(sensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            mapper.Map<SensorCreatedEvent>(sensor), 
            cancellationToken);

        return Result<Guid>.Success(result);
    }

    public async Task<Result> DeleteSensorAsync(
        Guid sensorId,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository
            .GetByIdAsync(sensorId, cancellationToken);

        if (existingSensor is null)
        {
            return Result.Failure(Error.NotFound(
                    "Sensor.NotFound",
                    $"{nameof(SensorEntity)} {sensorId} not found"));
        }

        var controller = await controllerRepository
            .GetByIdAsync(existingSensor.ControllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {existingSensor.ControllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        await sensorRepository.DeleteAsync(sensorId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new SensorDeletedEvent
        {
            SensorId = sensorId,
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<SensorResponseDto>>> GetAllSensorsAsync(
        SensorFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var specification = new SensorFilterSpecification(
            new SensorFilterParams
            {
                UserId = userContext.UserId,
                ControllerId = filter.ControllerId,
                Type = filter.Type,
                State = filter.State,
            });

        var sensors = await sensorRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        return Result<IReadOnlyList<SensorResponseDto>>.Success(
            mapper.Map<IReadOnlyList<SensorResponseDto>>(sensors));
    }

    public async Task<Result<SensorResponseDto>> GetSensorByIdAsync(
        Guid sensorId,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository
            .GetByIdAsync(sensorId, cancellationToken);

        if (existingSensor is null)
        {
            return Result<SensorResponseDto>
                .Failure(Error.NotFound(
                    "Sensor.NotFound",
                    $"{nameof(SensorEntity)} {sensorId} not found"));
        }

        var controller = await controllerRepository
            .GetByIdAsync(existingSensor.ControllerId, cancellationToken);

        if (controller is null)
        {
            return Result<SensorResponseDto>
                .Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {existingSensor.ControllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result<SensorResponseDto>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        return Result<SensorResponseDto>.Success(
            mapper.Map<SensorResponseDto>(existingSensor));
    }

    public async Task<Result> SetSensorStateAsync(
        Guid sensorId,
        SensorStateEnum state,
        CancellationToken cancellationToken)
    {
        var existingSensor = await sensorRepository
            .GetByIdAsync(sensorId, cancellationToken);

        if (existingSensor is null)
        {
            return Result.Failure(Error.NotFound(
                    "Sensor.NotFound",
                    $"{nameof(SensorEntity)} {sensorId} not found"));
        }

        var controller = await controllerRepository
            .GetByIdAsync(existingSensor.ControllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {existingSensor.ControllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        existingSensor.SetState(state);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            mapper.Map<SensorStateChangedCommand>(existingSensor), 
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateSensorAsync(
        Guid sensorId,
        SensorUpdateRequestDto updateRequestDto,
        CancellationToken cancellationToken)
    {
        var validationResult = updateValidator.Validate(updateRequestDto);

        if (!validationResult.IsValid)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "CreateRequest.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var existingSensor = await sensorRepository
            .GetByIdAsync(sensorId, cancellationToken);

        if (existingSensor is null)
        {
            return Result.Failure(Error.NotFound(
                    "Sensor.NotFound",
                    $"{nameof(SensorEntity)} {sensorId} not found"));
        }

        var controller = await controllerRepository
            .GetByIdAsync(existingSensor.ControllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {existingSensor.ControllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        var errors = existingSensor.Update(
            updateRequestDto.ConnectionProtocol,
            updateRequestDto.ConnectionAddress,
            updateRequestDto.ControllerId,
            updateRequestDto.Type,
            updateRequestDto.Unit);

        if (errors is not null)
        {
            return Result.Failure(Error.Validation(
                    "Sensor.Invalid",
                    $"Failed to update {nameof(SensorEntity)}: {string.Join(", ", errors!)}"));
        }

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            mapper.Map<SensorUpdatedEvent>(existingSensor), 
            cancellationToken);

        return Result.Success();
    }
}
