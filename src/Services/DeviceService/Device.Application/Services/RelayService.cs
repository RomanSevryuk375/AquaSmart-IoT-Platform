using AutoMapper;
using Contracts.Results;
using Device.Application.DTOs.Relay;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.Domain.SpecificationParams;
using Device.Domain.Specifications;
using FluentValidation;

namespace Device.Application.Services;

public sealed class RelayService(
    IRelayRepository relayRepository,
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IValidator<RelayUpdateRequestDto> updateValidator,
    IValidator<RelayRequestDto> createValidator,
    IUserContext userContext,
    IDeviceSecurityService securityService) : IRelayService
{
    public async Task<Result<RelayResponseDto>> AddRelayAsync(
        RelayRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = createValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result<RelayResponseDto>
                .Failure(Error.Validation(
                    "CreateRequest.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            request.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<RelayResponseDto>.Failure(ownership.Error);
        }

        var relay = RelayEntity.Create(
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

        if (relay.IsFailure)
        {
            return Result<RelayResponseDto>.Failure(relay.Error);
        }

        await relayRepository.AddAsync(relay.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RelayResponseDto>.Success(
            mapper.Map<RelayResponseDto>(relay.Value));
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

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        existingRelay.MarkAsDeleted();

        await relayRepository.DeleteAsync(relayId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<RelayResponseDto>.Failure(ownership.Error);
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
            return Result
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

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        if (updateRequestDto.ControllerId != existingRelay.ControllerId)
        {
            var newControllerOwnership = await securityService.EnsureUserOwnsControllerAsync(
                updateRequestDto.ControllerId, cancellationToken);

            if (newControllerOwnership.IsFailure)
            {
                return newControllerOwnership;
            }
        }

        var result = existingRelay.Update(
            updateRequestDto.ControllerId,
            updateRequestDto.ConnectionProtocol,
            updateRequestDto.ConnectionAddress,
            updateRequestDto.Purpose,
            updateRequestDto.IsNormalyOpen);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
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

        return Result.Success();
    }
}
