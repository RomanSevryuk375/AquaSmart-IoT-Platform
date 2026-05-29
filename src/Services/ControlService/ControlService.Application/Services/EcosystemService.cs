using AutoMapper;
using Contracts.Events.EcosystemEvents;
using Contracts.Results;
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
    ISecureService secureService,
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
        var ownership = await secureService.EnsureUserOwnsEcosystemAsync(
            ecosystemId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<EcosystemResponseDto>
                .Failure(ownership.Error);
        }

        return Result<EcosystemResponseDto>.Success(
            mapper.Map<EcosystemResponseDto>(ownership.Value));
    }

    public async Task<Result<Guid>> CreateEcosystemAsync(
        EcosystemRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if(!validationResult.IsValid)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Ecosystem.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var createResult = EcosystemEntity.Create(
            userContext.UserId,
            request.Type,
            request.Name,
            request.Volume,
            request.ControllerId);
        if (createResult.IsFailure)
        {
            return Result<Guid>.Failure(createResult.Error);
        }

        var result = await ecosystemRepository.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result);
    }

    public async Task<Result> UpdateEcosystemAsync(
        Guid ecosystemId,
        EcosystemUpdateRequestDto request,
        CancellationToken cancellationToken)
    {
        var ownership = await secureService.EnsureUserOwnsEcosystemAsync(
            ecosystemId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        var ecosystem = ownership.Value;
        var nameResult = ecosystem.SetName(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        var volumeResult = ecosystem.SetVolume(request.Volume);
        if (volumeResult.IsFailure)
        {
            return Result.Failure(volumeResult.Error);
        }

        await ecosystemRepository.UpdateAsync(ecosystem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteEcosystemAsync(
        Guid ecosystemId,
        CancellationToken cancellationToken)
    {
        var ownership = await secureService.EnsureUserOwnsEcosystemAsync(
            ecosystemId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        await ecosystemRepository.DeleteAsync(ecosystemId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            new EcosystemDeletedEvent { EcosystemId = ecosystemId },
            cancellationToken);

        return Result.Success();
    }
}
