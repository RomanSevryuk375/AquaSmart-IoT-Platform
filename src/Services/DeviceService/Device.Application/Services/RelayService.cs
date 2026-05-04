using AutoMapper;
using Contracts.Events.RelayEvents;
using Contracts.Results;
using Device.Application.DTOs.Relay;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.Domain.SpecificationParams;
using Device.Domain.Specifications;
using FluentValidation;
using MassTransit;
using Error = Contracts.Results.Error;

namespace Device.Application.Services;

public sealed class RelayService(
    IControllerRepository controllerRepository,
    IRelayRepository relayRepository,
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IPublishEndpoint publisherEndpoint,
    IValidator<RelayUpdateRequestDto> updateValidator,
    IValidator<RelayRequestDto> createValidator,
    IUserContext userContext) : IRelayService
{
    public async Task<Result<Guid>> AddRelayAsync(
        RelayRequestDto request,
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

        var existingController = await controllerRepository
            .GetByIdAsync(request.ControllerId, cancellationToken);

        if (existingController is null)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {request.ControllerId} not found"));
        }

        if (existingController.UserId != userContext.UserId)
        {
            return Result<Guid>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        var (relay, errors) = RelayEntity.Create(
            request.ControllerId,
            userContext.UserId,
            request.PowerSensorId,
            request.Name,
            request.ConnectionProtocol,
            request.ConnectionAddress,
            request.IsNormalyOpen,
            request.Purpose,
            request.IsActive,
            request.IsManual);

        if (relay is null)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "Relay.Invalid",
                    $"Failed to create {nameof(RelayEntity)}: {string.Join(", ", errors!)}"));
        }

        var result = await relayRepository.AddAsync(relay, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisherEndpoint.Publish(
            mapper.Map<RelayCreatedEvent>(relay),
            cancellationToken);

        return Result<Guid>.Success(result);
    }

    public async Task<Result> DeleteRelayAsync(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken);

        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"{nameof(RelayEntity)} {relayId} not found"));
        }

        var controller = await controllerRepository
            .GetByIdAsync(existingRelay.ControllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {existingRelay.ControllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        await relayRepository.DeleteAsync(relayId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisherEndpoint.Publish(new RelayDeletedEvent
            {
                RelayId = relayId,
            }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<RelayResponseDto>>> GetAllRelaysAsync(
        RelayFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var specification = new RelayFilterSpecification(
            new RelayFilterParams
            {
                UserId = userContext.UserId,
                ControllerId = filter.ControllerId,
                Purpose = filter.Purpose,
                IsActive = filter.IsActive,
                IsManual = filter.IsManual,
            });

        var relays = await relayRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        return Result<IReadOnlyList<RelayResponseDto>>.Success(
            mapper.Map<IReadOnlyList<RelayResponseDto>>(relays));
    }

    public async Task<Result<RelayResponseDto>> GetRelayByIdAsync(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken);

        if (existingRelay is null)
        {
            return Result<RelayResponseDto>
                .Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"{nameof(RelayEntity)} {relayId} not found"));
        }

        var controller = await controllerRepository
            .GetByIdAsync(existingRelay.ControllerId, cancellationToken);

        if (controller == null || 
            controller.UserId != userContext.UserId)
        {
            return Result<RelayResponseDto>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        return Result<RelayResponseDto>.Success(
            mapper.Map<RelayResponseDto>(existingRelay));
    }

    public async Task<Result> UpdateRelayAsync(
        Guid relayId,
        RelayUpdateRequestDto updateRequestDto,
        CancellationToken cancellationToken)
    {
        var validationResult = updateValidator.Validate(updateRequestDto);

        if (!validationResult.IsValid)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "UpdateRequest.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken);

        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"{nameof(RelayEntity)} {relayId} not found"));
        }

        var controller = await controllerRepository
            .GetByIdAsync(updateRequestDto.ControllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {updateRequestDto.ControllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        var errors = existingRelay.Update(
            updateRequestDto.ControllerId,
            updateRequestDto.ConnectionProtocol,
            updateRequestDto.ConnectionAddress,
            updateRequestDto.Purpose,
            updateRequestDto.IsNormalyOpen);

        if (errors is not null)
        {
            return Result.Failure(Error.Validation(
                    "Relay.Invalid",
                    $"Failed to update {nameof(RelayEntity)}: {string.Join(", ", errors!)}"));
        }

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisherEndpoint.Publish(
            mapper.Map<RelayUpdatedEvent>(existingRelay),
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SetRelayPowerSensorAsync(
        Guid relayId,
        Guid powerSensorId,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken);

        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"{nameof(RelayEntity)} {relayId} not found"));
        }

        var existingSensor = await sensorRepository
            .GetByIdAsync(powerSensorId, cancellationToken);

        if (existingSensor is null)
        {
            return Result.Failure(Error.NotFound(
                    "Sensor.NotFound",
                    $"{nameof(SensorEntity)} {powerSensorId} not found"));
        }

        var controller = await controllerRepository
            .GetByIdAsync(existingRelay.ControllerId, cancellationToken);

        if (controller is null)
        {
            return Result<bool>
                .Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {existingRelay.ControllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result<bool>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        if (existingRelay.ControllerId != existingSensor.ControllerId)
        {
            return Result.Failure(Error.Validation(
                "Relay.Invalid",
                "Sensor and Relay must belong to the same controller"));
        }

        existingRelay.SetPowerSensor(powerSensorId);

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisherEndpoint.Publish(new SetRelayPowerSensorEvent
        {
            RelayId = relayId,
            PowerSensorId = powerSensorId,
        }, cancellationToken);

        return Result.Success();
    }
}
