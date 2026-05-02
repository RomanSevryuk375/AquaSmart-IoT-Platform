using AutoMapper;
using Contracts.Events.RelayEvents;
using Contracts.Exceptions;
using Device.Application.DTOs.Relay;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.Domain.SpecificationParams;
using Device.Domain.Specifications;
using FluentValidation;
using MassTransit;

namespace Device.Application.Services;

public sealed class RelayService(
    IControllerRepository controllerRepository,
    IRelayRepository relayRepository,
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IPublishEndpoint publisherEndpoint,
    IValidator<RelayUpdateRequestDto> updateValidator,
    IValidator<RelayRequestDto> createValidator) : IRelayService
{
    public async Task<Guid> AddRelayAsync(
        RelayRequestDto request,
        CancellationToken cancellationToken)
    {
        createValidator.ValidateAndThrow(request);

        var existingController = await controllerRepository
            .GetByIdAsync(request.ControllerId, cancellationToken)
            ?? throw new NotFoundException($"Controller {request.ControllerId} not found");

        var (relay, errors) = RelayEntity.Create(
            request.ControllerId,
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
            throw new DomainValidationException(
                $"Failed to create {nameof(RelayEntity)}: {string.Join(", ", errors!)}");
        }

        var result = await relayRepository.AddAsync(relay, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisherEndpoint.Publish(
            mapper.Map<RelayCreatedEvent>(relay),
            cancellationToken);

        return result;
    }

    public async Task DeleteRelayAsync(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        await relayRepository.DeleteAsync(relayId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisherEndpoint.Publish(new RelayDeletedEvent
        {
            RelayId = relayId,
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<RelayResponseDto>> GetAllRelaysAsync(
        RelayFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var specification = new RelayFilterSpecification(
            new RelayFilterParams
            {
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

        return mapper.Map<IReadOnlyList<RelayResponseDto>>(relays);
    }

    public async Task<RelayResponseDto> GetRelayByIdAsync(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken)
            ?? throw new NotFoundException($"Relay {relayId} not found");

        return mapper.Map<RelayResponseDto>(existingRelay);
    }

    public async Task UpdateRelayAsync(
        Guid relayId,
        RelayUpdateRequestDto updateRequestDto,
        CancellationToken cancellationToken)
    {
        updateValidator.Validate(updateRequestDto);

        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken)
            ?? throw new NotFoundException(
                $"{nameof(RelayEntity)} {relayId} not found");

        var controller = await controllerRepository
            .GetByIdAsync(updateRequestDto.ControllerId, cancellationToken)
            ?? throw new NotFoundException(
                $"{nameof(ControllerEntity)} {updateRequestDto.ControllerId} not found");

        var errors = existingRelay.Update(
            updateRequestDto.ControllerId,
            updateRequestDto.ConnectionProtocol,
            updateRequestDto.ConnectionAddress,
            updateRequestDto.Purpose,
            updateRequestDto.IsNormalyOpen);

        if (errors is not null && errors.Count > 0)
        {
            throw new DomainValidationException(
                $"Update failed: {string.Join(", ", errors)}");
        }

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisherEndpoint.Publish(
            mapper.Map<RelayUpdatedEvent>(errors),
            cancellationToken);
    }

    public async Task SetRelayPowerSensorAsync(
        Guid relayId,
        Guid powerSensorId,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken)
            ?? throw new NotFoundException(
                $"{nameof(RelayEntity)} {relayId} not found");

        var existingSensor = await sensorRepository
            .GetByIdAsync(powerSensorId, cancellationToken)
            ?? throw new NotFoundException($"Sensor {powerSensorId} not found");

        if (existingRelay.ControllerId != existingSensor.ControllerId)
        {
            throw new DomainValidationException(
                "Sensor and Relay must belong to the same controller");
        }

        existingRelay.SetPowerSensor(powerSensorId);

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisherEndpoint.Publish(new SetRelayPowerSensorEvent
        {
            RelayId = relayId,
            PowerSensorId = powerSensorId,
        }, cancellationToken);
    }
}
