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
    IRelayRepository relayRepository,
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
        var specification = new AutomationRuleFilterSpecification(
            new AutomationRuleFilterParams
            {
                EcosystemId = filter.EcosystemId,
                RelayId = filter.RelayId,
                Action = filter.Action,
                Operator = filter.Operator,
            });

        if (filter.EcosystemId.HasValue)
        {
            var ownership = await secureService
                .EnsureUserOwnsEcosystemAsync(filter.EcosystemId.Value, cancellationToken);

            if (ownership.IsFailure)
            {
                return Result<IReadOnlyList<AutomationRuleResponseDto>>
                    .Failure(ownership.Error);
            }
        }
        else
        {
            return Result<IReadOnlyList<AutomationRuleResponseDto>>
                .Failure(Error.Validation(
                    "Rule.FilterRequired", 
                    "EcosystemId filter is required for security reasons."));
        }

        var rules = await ruleRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        return Result<IReadOnlyList<AutomationRuleResponseDto>>
            .Success(mapper.Map<IReadOnlyList<AutomationRuleResponseDto>>(rules));
    }

    public async Task<Result<AutomationRuleResponseDto>> GetRuleByIdAsync(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository
            .GetByIdAsync(ruleId, cancellationToken);

        if (rule is null)
        {
            return Result<AutomationRuleResponseDto>
                .Failure(Error.NotFound(
                    "Rule.NotFound",
                    $"Rule {ruleId} not found"));
        }

        var ownership = await secureService
                .EnsureUserOwnsEcosystemAsync(rule.EcosystemId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<AutomationRuleResponseDto>
                .Failure(ownership.Error);
        }

        return Result<AutomationRuleResponseDto>
            .Success(mapper.Map<AutomationRuleResponseDto>(rule));
    }

    public async Task<Result<Guid>> CreateRuleAsync(
        AutomationRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator
            .ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Rule.Invalid",
                    string.Join(", ", validationResult.Errors)));
        }

        var ownership = await secureService
                .EnsureUserOwnsEcosystemAsync(request.EcosystemId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<Guid>
                .Failure(ownership.Error);
        }

        var existingRelay = await relayRepository
            .GetByIdAsync(request.RelayId, cancellationToken);

        if (existingRelay is null)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"Relay {request.RelayId} not found"));
        }

        if (existingRelay.EcosystemId != request.EcosystemId)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "Rule.InvalidRelay",
                    "Target relay must belong to the same ecosystem as the rule."));
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
        var rule = await ruleRepository
            .GetByIdAsync(ruleId, cancellationToken);

        if (rule is null)
        {
            return Result.Failure(Error.NotFound(
                    "Rule.NotFound",
                    $"Rule {ruleId} not found"));
        }

        var ownership = await secureService
                .EnsureUserOwnsEcosystemAsync(rule.EcosystemId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        var validationResult = rule.Update(
            request.Name,
            request.RelayId,
            request.Operator,
            request.Action);

        if (validationResult.IsFailure)
        {
            return Result.Failure(Error.Validation(
                    "Rule.Invalid",
                    string.Join(", ", validationResult.Error)));
        }

        await ruleRepository.UpdateAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteRuleAsync(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository
            .GetByIdAsync(ruleId, cancellationToken);

        if (rule is null)
        {
            return Result.Failure(Error.NotFound(
                    "Rule.NotFound",
                    $"Rule {ruleId} not found"));
        }

        var ownership = await secureService
                .EnsureUserOwnsEcosystemAsync(rule.EcosystemId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        await ruleRepository.DeleteAsync(ruleId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}