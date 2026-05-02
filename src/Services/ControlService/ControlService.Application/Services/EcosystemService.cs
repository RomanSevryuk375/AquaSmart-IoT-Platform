using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Application.DTOs.Ecosystem;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Control.Domain.SpecificationParams;
using Control.Domain.Specifications;
using FluentValidation;
using MassTransit;

namespace Control.Application.Services;

public sealed class EcosystemService(
    IEcosystemRepository ecosystemRepository,
    IPublishEndpoint publishEndpoint,
    IUserContext userContext,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IValidator<EcosystemRequestDto> validator) : IEcosystemService
{
    public async Task<IReadOnlyList<EcosystemResponseDto>> GetAllEcosystemsAsync(
        EcosystemFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var specification = new EcosystemFilterSpecification(
            new EcosystemFilterParams
            {
                UserId = userContext.UserId,
                Name = filter.Name,
                ControllerId = filter.ControllerId,
                Type = filter.Type,
            });

        var ecosystems = await ecosystemRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        return mapper.Map<IReadOnlyList<EcosystemResponseDto>>(ecosystems);
    }

    public async Task<Result<EcosystemResponseDto>> GetEcosystemByIdAsync(
        Guid ecosystemId,
        CancellationToken cancellationToken)
    {
        var ecosystem = await ecosystemRepository
            .GetByIdAsync(ecosystemId, cancellationToken);

        if (ecosystem is null)
        {
            return Result<EcosystemResponseDto>
                .Failure(Error.NotFound(
                    "Ecosystem.NotFound",
                    $"{nameof(EcosystemEntity)} not found"));
        }

        if (ecosystem.UserId != userContext.UserId)
        {
            return Result<EcosystemResponseDto>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
        }

        return Result<EcosystemResponseDto>
            .Success(mapper.Map<EcosystemResponseDto>(ecosystem));
    }

    public async Task<Result<Guid>> CreateEcosystemAsync(
        EcosystemRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if(!validationResult.IsValid)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Ecosystem.Invalid",
                    $"Failed to validate {nameof(EcosystemEntity)}: {string.Join(", ", validationResult.Errors)}"));
        }

        var (ecosystem, errors) = EcosystemEntity.Create(
            userContext.UserId,
            request.Type,
            request.Name,
            request.Volume,
            request.ControllerId);

        if (ecosystem is null)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Ecosystem.Invalid",
                    $"Failed to create {nameof(EcosystemEntity)}: {string.Join(", ", errors)}"));
        }

        var result = await ecosystemRepository.AddAsync(ecosystem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            mapper.Map<EcosystemCreatedEvent>(ecosystem),
            cancellationToken);

        return Result<Guid>.Success(result);
    }

    public async Task<Result> UpdateEcosystemAsync(
        Guid ecosystemId,
        EcosystemUpdateRequestDto request,
        CancellationToken cancellationToken)
    {
        var ecosystem = await ecosystemRepository
            .GetByIdAsync(ecosystemId, cancellationToken);

        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound(
                "Ecosystem.NotFound",
                $"{nameof(EcosystemEntity)} not found"));
        }

        if (ecosystem.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
        }

        var nameErrors = ecosystem.SetName(request.Name);

        if (nameErrors is not null)
        {
            return Result
                .Failure(Error.Validation("Ecosystem.InvalidName", nameErrors[0]));
        }

        var volumeErrors = ecosystem.SetVolume(request.Volume);

        if (volumeErrors is not null)
        {
            return Result
                .Failure(Error.Validation("Ecosystem.InvalidVolume", volumeErrors[0]));
        }

        await ecosystemRepository.UpdateAsync(ecosystem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            mapper.Map<EcosystemUdatedEvent>(ecosystem),
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteEcosystemAsync(
        Guid ecosystemId,
        CancellationToken cancellationToken)
    {
        var ecosystem = await ecosystemRepository
            .GetByIdAsync(ecosystemId, cancellationToken);

        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound(
                    "Ecosystem.NotFound",
                    $"{nameof(EcosystemEntity)} not found"));
        }

        if (ecosystem.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
        }

        await ecosystemRepository.DeleteAsync(ecosystemId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            new EcosystemDeletedEvent { EcosystemId = ecosystemId },
            cancellationToken);

        return Result.Success();
    }
}
