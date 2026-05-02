using AutoMapper;
using Contracts.Events.ControllerEvents;
using Contracts.Exceptions;
using Device.Application.DTOs.Controller;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.Domain.SpecificationParams;
using Device.Domain.Specifications;
using FluentValidation;
using MassTransit;

namespace Device.Application.Services;

public sealed class ControllerService(
    IControllerRepository controllerRepository,
    IPublishEndpoint publishEndpoint,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    IMyHasher myHasher,
    IMapper mapper,
    IValidator<ControllerRequestDto> createValidator,
    IValidator<ControllerUpdateRequestDto> updateValidator) : IControllerService
{
    public async Task<ControllerRegistredResponseDto> AddControllerAsync(
        ControllerRequestDto request, 
        CancellationToken cancellationToken)
    {
        createValidator.ValidateAndThrow(request);
                
        var deviceToken = Guid.NewGuid().ToString();

        var (controller, errors) = ControllerEntity.Create(
            userContext.UserId,
            request.MacAddress,
            myHasher.Generate(deviceToken),
            request.Name,
            request.IsOnline);

        if (controller is null)
        {
            throw new DomainValidationException(
                $"Failed to create {nameof(ControllerEntity)}: {string.Join(", ", errors!)}");
        }

        var result = await controllerRepository.AddAsync(controller, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ControllerRegistredResponseDto
        {
            ControllerId = result,
            DeviceToken = deviceToken
        };
    }

    public async Task DeleteControllerAsync(
        Guid controllerId, 
        CancellationToken cancellationToken)
    {
        await controllerRepository.DeleteAsync(controllerId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ControllerResponseDto>> GetAllControllersAsync(
        ControllerFilterDto filter, 
        int? skip, 
        int? take, 
        CancellationToken cancellationToken)
    {
        
        var specification = new ControllerFilterSpecification(
            new ControllerFilterParams
            {
                SearchTerm = filter.SearchTerm,
                IsOnline = filter.IsOnline,
            });

        var controllers = await controllerRepository.GetAllAsync(
            specification, 
            skip, 
            take, 
            cancellationToken);

        return mapper.Map<IReadOnlyList<ControllerResponseDto>>(controllers);
    }

    public async Task<ControllerResponseDto> GetControllerByIdAsync(
        Guid controllerId, 
        CancellationToken cancellationToken)
    {
        var controller = await controllerRepository
            .GetByIdAsync(controllerId, cancellationToken)
            ?? throw new NotFoundException($"{nameof(ControllerEntity)} not found");

        return mapper.Map<ControllerResponseDto>(controller);
    }

    public async Task<ControllerPingResponseDto> PingControllerAsync(
        Guid controllerId,
        string deviceToken, 
        CancellationToken cancellationToken)
    {
        var controller = await controllerRepository
            .GetByIdAsync(controllerId, cancellationToken)
            ?? throw new NotFoundException($"{nameof(ControllerEntity)} not found");

        var verify = myHasher
            .Verify(deviceToken, controller.DeviceTokenHash);

        if (!verify)
        {
            throw new InvalidCredentialsException("DeviceToken is not verified.");
        }

        controller.RecordPing();

        await controllerRepository.UpdateAsync(controller, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ControllerPingResponseDto();
    }

    public async Task<bool> ToggleControllerStateAsync(
        Guid controllerId, 
        CancellationToken cancellationToken)
    {
        var controller = await controllerRepository
            .GetByIdAsync(controllerId, cancellationToken)
            ?? throw new NotFoundException($"{nameof(ControllerEntity)} not found");

        controller.ToggleState();

        await controllerRepository.UpdateAsync(controller, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!controller.IsOnline)
        {
            await publishEndpoint.Publish(new ControllerNotOnlineEvent 
            { 
                UserId = controller.UserId,
                ControllerId = controller.Id,
                LastSeenAt = controller.LastSeenAt,
            }, cancellationToken);
        }

        return controller.IsOnline;
    }

    public async Task UpdateControllerAsync(
        Guid controllerId,
        ControllerUpdateRequestDto updateRequestDto, 
        CancellationToken cancellationToken)
    {
        updateValidator.ValidateAndThrow(updateRequestDto);

        var controller = await controllerRepository
            .GetByIdAsync(controllerId, cancellationToken)
            ?? throw new NotFoundException($"{nameof(ControllerEntity)} not found");

        var errors = controller.Update(
            updateRequestDto.MacAddress,
            updateRequestDto.Name);

        if (errors is not null && errors.Count > 0)
        {
            throw new DomainValidationException(
                $"Failed to create {nameof(ControllerEntity)}: {string.Join(", ", errors)}");
        }

        await controllerRepository.UpdateAsync(controller, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
