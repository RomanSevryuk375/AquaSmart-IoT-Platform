using AutoMapper;
using Contracts.Events.ControllerEvents;
using Contracts.Results;
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
    public async Task<Result<ControllerRegistredResponseDto>> AddControllerAsync(
        ControllerRequestDto request, 
        CancellationToken cancellationToken)
    {
        var validationResult = createValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result<ControllerRegistredResponseDto>
                .Failure(Error.Validation(
                    "UpdateRequest.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var deviceToken = Guid.NewGuid().ToString();

        var (controller, errors) = ControllerEntity.Create(
            userContext.UserId,
            request.MacAddress,
            myHasher.Generate(deviceToken),
            request.Name,
            request.IsOnline);

        if (controller is null)
        {
            return Result<ControllerRegistredResponseDto>
                .Failure(Error.Validation(
                    "ConstrollerRequest.Invalid",
                    $"Failed to create {nameof(ControllerEntity)}: {string.Join(", ", errors!)}"));
        }

        var result = await controllerRepository.AddAsync(controller, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ControllerRegistredResponseDto>.Success(
            new ControllerRegistredResponseDto
            {
                ControllerId = result,
                DeviceToken = deviceToken
            });
    }

    public async Task<Result> DeleteControllerAsync(
        Guid controllerId, 
        CancellationToken cancellationToken)
    {
        await controllerRepository.DeleteAsync(controllerId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ControllerResponseDto>>> GetAllControllersAsync(
        ControllerFilterDto filter, 
        int? skip, 
        int? take, 
        CancellationToken cancellationToken)
    {
        
        var specification = new ControllerFilterSpecification(
            new ControllerFilterParams
            {
                UserId = userContext.UserId,
                SearchTerm = filter.SearchTerm,
                IsOnline = filter.IsOnline,
            });

        var controllers = await controllerRepository.GetAllAsync(
            specification, 
            skip, 
            take, 
            cancellationToken);

        return Result<IReadOnlyList<ControllerResponseDto>>.Success(
            mapper.Map<IReadOnlyList<ControllerResponseDto>>(controllers));
    }

    public async Task<Result<ControllerResponseDto>> GetControllerByIdAsync(
        Guid controllerId, 
        CancellationToken cancellationToken)
    {
        var controller = await controllerRepository
            .GetByIdAsync(controllerId, cancellationToken);

        if (controller is null)
        {
            return Result<ControllerResponseDto>
                .Failure(Error.NotFound(
                    "Controller.NotFound", 
                    $"{nameof(ControllerEntity)} {controllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result<ControllerResponseDto>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        return Result<ControllerResponseDto>.Success(
            mapper.Map<ControllerResponseDto>(controller));
    }

    public async Task<Result<ControllerPingResponseDto>> PingControllerAsync(
        Guid controllerId,
        string deviceToken, 
        CancellationToken cancellationToken)
    {
        var controller = await controllerRepository
            .GetByIdAsync(controllerId, cancellationToken);

        if (controller is null)
        {
            return Result<ControllerPingResponseDto>
                .Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {controllerId} not found"));
        }

        var verify = myHasher
            .Verify(deviceToken, controller.DeviceTokenHash);

        if (!verify)
        {
            return Result<ControllerPingResponseDto>
                .Failure(Error.Conflict(
                    "Controller.Forbidden",
                    $"invalid device token"));
        }

        controller.RecordPing();

        await controllerRepository.UpdateAsync(controller, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ControllerPingResponseDto>.Success(new ControllerPingResponseDto());
    }

    public async Task<Result<bool>> ToggleControllerStateAsync(
        Guid controllerId, 
        CancellationToken cancellationToken)
    {
        var controller = await controllerRepository
            .GetByIdAsync(controllerId, cancellationToken);

        if (controller is null)
        {
            return Result<bool>
                .Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)}  {controllerId} not found"));
        }

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

        return Result<bool>.Success(controller.IsOnline);
    }

    public async Task<Result> UpdateControllerAsync(
        Guid controllerId,
        ControllerUpdateRequestDto updateRequestDto, 
        CancellationToken cancellationToken)
    {
        var result = updateValidator.Validate(updateRequestDto);

        if (!result.IsValid)
        {
            return Result.Failure(Error.Validation(
                    "UpdateRequest.Invalid",
                    string.Join(", ", result.Errors)));
        }

        var controller = await controllerRepository
            .GetByIdAsync(controllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound(
                    "Controller.NotFound",
                    $"{nameof(ControllerEntity)} {controllerId} not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result<ControllerResponseDto>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this controller"));
        }

        var errors = controller.Update(
            updateRequestDto.MacAddress,
            updateRequestDto.Name);

        if (errors is not null)
        {
            return Result.Failure(Error.Validation(
                    "ConstrollerRequest.Invalid",
                    $"Failed to update {nameof(ControllerEntity)}: {string.Join(", ", errors!)}"));
        }

        await controllerRepository.UpdateAsync(controller, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
