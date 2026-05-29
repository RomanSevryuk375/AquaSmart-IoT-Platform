using AutoMapper;
using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Control.Domain.SpecificationParams;
using Control.Domain.Specifications;
using FluentValidation;

namespace Control.Application.Services;

public sealed class AutomationRuleService(
    IAutomationRuleRepository ruleRepository,
    ISecureService secureService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IValidator<AutomationRuleRequestDto> validator) : IAutomationRuleService
{
    public async Task<Result<IReadOnlyList<AutomationRuleResponseDto>>> GetAllRulesAsync(
        AutomationRuleFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        if (!filter.EcosystemId.HasValue)
        {
            return Result<IReadOnlyList<AutomationRuleResponseDto>>
                .Failure(Error.Validation(
                    "Rule.FilterRequired", 
                    "EcosystemId filter is required for security reasons."));
        }

        var ownership = await secureService.EnsureUserOwnsEcosystemAsync(
            filter.EcosystemId.Value, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<IReadOnlyList<AutomationRuleResponseDto>>
                .Failure(ownership.Error);
        }

        var specification = new AutomationRuleFilterSpecification(
            new AutomationRuleFilterParams
            {
                EcosystemId = filter.EcosystemId,
                RelayId = filter.RelayId,
                Action = filter.Action,
                Operator = filter.Operator,
            });

        var rules = await ruleRepository.GetAllAsync(
            specification, skip, take, cancellationToken);

        return Result<IReadOnlyList<AutomationRuleResponseDto>>.Success(
            mapper.Map<IReadOnlyList<AutomationRuleResponseDto>>(rules));
    }

    public async Task<Result<AutomationRuleResponseDto>> GetRuleByIdAsync(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var ruleResult = await GetValidRuleAsync(ruleId, cancellationToken);
        if (ruleResult.IsFailure)
        {
            return Result<AutomationRuleResponseDto>.Failure(ruleResult.Error);
        }

        var rule = ruleResult.Value;

        return Result<AutomationRuleResponseDto>
            .Success(mapper.Map<AutomationRuleResponseDto>(rule));
    }

    public async Task<Result<Guid>> CreateRuleAsync(
        AutomationRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(
            request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "Rule.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var ownership = await secureService.EnsureUserOwnsEcosystemAsync(
            request.EcosystemId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<Guid>
                .Failure(ownership.Error);
        }

        var relayOwnership = await secureService.EnsureEcosystemOwnsRelayAsync(
            request.EcosystemId, request.RelayId, cancellationToken);
        if (relayOwnership.IsFailure)
        {
            return Result<Guid>
                .Failure(relayOwnership.Error);
        }

        var createResult = AutomationRuleEntity.Create(
            request.EcosystemId,
            request.Name,
            request.RelayId,
            request.Operator,
            request.Action,
            request.IsActive);

        if (createResult.IsFailure)
        {
            return Result<Guid>.Failure(createResult.Error);
        }

        var result = await ruleRepository.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result);
    }

    public async Task<Result> UpdateRuleAsync(
        Guid ruleId,
        AutomationRuleUpdateRequestDto request,
        CancellationToken cancellationToken)
    {
        var ruleResult = await GetValidRuleAsync(ruleId, cancellationToken);
        if (ruleResult.IsFailure)
        {
            return Result.Failure(ruleResult.Error);
        }

        var relayOwnership = await secureService.EnsureEcosystemOwnsRelayAsync(
            ruleResult.Value.EcosystemId, request.RelayId, cancellationToken);
        if (relayOwnership.IsFailure)
        {
            return Result.Failure(relayOwnership.Error);
        }

        var validationResult = ruleResult.Value.Update(
            request.Name,
            request.RelayId,
            request.Operator,
            request.Action);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        var rule = ruleResult.Value;
        await ruleRepository.UpdateAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteRuleAsync(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var ruleResult = await GetValidRuleAsync(ruleId, cancellationToken);
        if (ruleResult.IsFailure)
        {
            return Result.Failure(ruleResult.Error);
        }

        var rule = ruleResult.Value;

        await ruleRepository.DeleteAsync(rule.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<AutomationRuleEntity>> GetValidRuleAsync(
        Guid ruleId, 
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository
            .GetByIdAsync(ruleId, cancellationToken);
        if (rule is null)
        {
            return Result<AutomationRuleEntity>
                .Failure(Error.NotFound(
                    "Rule.NotFound",
                    $"Rule {ruleId} not found"));
        }

        var ownership = await secureService
            .EnsureUserOwnsEcosystemAsync(rule.EcosystemId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<AutomationRuleEntity>.Failure(ownership.Error);
        }

        return Result<AutomationRuleEntity>.Success(rule);
    }
}