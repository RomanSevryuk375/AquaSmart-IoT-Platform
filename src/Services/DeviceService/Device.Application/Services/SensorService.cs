using AutoMapper;
using Contracts.Enums;
using Contracts.Results;
using Device.Application.DTOs.Sensor;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.Domain.SpecificationParams;
using Device.Domain.Specifications;
using FluentValidation;

namespace Device.Application.Services;

public sealed class SensorService(
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IValidator<SensorRequestDto> createValidator,
    IValidator<SensorUpdateRequestDto> updateValidator,
    IUserContext userContext,
    IDeviceSecurityService securityService) : ISensorService
{
    public async Task<Result<SensorResponseDto>> AddSensorAsync(
        SensorRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await createValidator
            .ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result<SensorResponseDto>
                .Failure(Error.Validation(
                    "CreateRequest.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            request.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<SensorResponseDto>.Failure(ownership.Error);
        }

        var sensor = SensorEntity.Create(
            request.ControllerId,
            userContext.UserId,
            request.Name,
            request.ConnectionProtocol,
            request.ConnectionAddress,
            request.Type,
            request.Unit);

        if (sensor.IsFailure)
        {
            return Result<SensorResponseDto>.Failure(sensor.Error);
        }

        await sensorRepository.AddAsync(sensor.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SensorResponseDto>.Success(
            mapper.Map<SensorResponseDto>(sensor.Value));
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

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingSensor.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        existingSensor.MarkAsDeleted();

        await sensorRepository.DeleteAsync(sensorId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingSensor.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<SensorResponseDto>.Failure(ownership.Error);
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

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingSensor.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        existingSensor.SetState(state);

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingSensor.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<SensorResponseDto>.Failure(ownership.Error);
        }

        if (updateRequestDto.ControllerId != existingSensor.ControllerId)
        {
            var newControllerOwnership = await securityService.EnsureUserOwnsControllerAsync(
                updateRequestDto.ControllerId, cancellationToken);

            if (newControllerOwnership.IsFailure)
            {
                return newControllerOwnership;
            }
        }

        var result = existingSensor.Update(
            updateRequestDto.ConnectionProtocol,
            updateRequestDto.ConnectionAddress,
            updateRequestDto.ControllerId,
            updateRequestDto.Type,
            updateRequestDto.Unit);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await sensorRepository.UpdateAsync(existingSensor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
