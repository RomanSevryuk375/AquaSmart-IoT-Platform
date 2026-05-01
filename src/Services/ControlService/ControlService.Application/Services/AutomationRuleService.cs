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
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IValidator<AutomationRuleRequestDto> validator,
    IUserContext userContext) : IAutomationRuleService
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
            var ecosystem = await ecosystemRepository
                .GetByIdAsync(filter.EcosystemId.Value, cancellationToken);

            if (ecosystem == null || 
                ecosystem.UserId != userContext.UserId)
            {
                return Result<IReadOnlyList<AutomationRuleResponseDto>>
                    .Failure(Error.Conflict(
                        "Access.Denied", 
                        "Forbidden or ecosystem not found"));
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

        var ecosystem = await ecosystemRepository
        .GetByIdAsync(rule.EcosystemId, cancellationToken);

        if (ecosystem is null)
        {
            return Result<AutomationRuleResponseDto>
                .Failure(Error.NotFound(
                    "Ecosystem.NotFound",
                    $"Ecosystem {rule.EcosystemId} not found. "));
        }

        if (ecosystem.UserId != userContext.UserId)
        {
            return Result<AutomationRuleResponseDto>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
        }

        return Result<AutomationRuleResponseDto>
            .Success(mapper.Map<AutomationRuleResponseDto>(rule));
    }

    public async Task<Result<Guid>> CreateRuleAsync(
        AutomationRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);

        var existingEcosystem = await ecosystemRepository
            .GetByIdAsync(request.EcosystemId, cancellationToken);

        if (existingEcosystem is null)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Ecosystem.NotFound",
                    $"Ecosystem {request.EcosystemId} not found"));
        }

        if (existingEcosystem.UserId != userContext.UserId)
        {
            return Result<Guid>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
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

        var (rule, errors) = AutomationRuleEntity.Create(
            request.EcosystemId,
            request.Name,
            request.RelayId,
            request.Operator,
            request.Action,
            request.IsActive);

        if (rule is null)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "Rule.Invalid",
                    $"Failed to create {nameof(AutomationRuleEntity)}: {string.Join(", ", errors)}"));
        }

        var result = await ruleRepository.AddAsync(rule!, cancellationToken);
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
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Rule.NotFound",
                    $"Rule {ruleId} not found"));
        }

        var ecosystem = await ecosystemRepository
        .GetByIdAsync(rule.EcosystemId, cancellationToken);

        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound(
                    "Ecosystem.NotFound",
                    $"Ecosystem {rule.EcosystemId} not found. "));
        }

        if (ecosystem.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
        }

        var errors = rule.Update(
            request.Name,
            request.RelayId,
            request.Operator,
            request.Action);

        if (errors is not null)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "Rule.Invalid",
                    $"Failed to update {nameof(AutomationRuleEntity)}: {string.Join(", ", errors)}"));
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
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Rule.NotFound",
                    $"Rule {ruleId} not found"));
        }

        var ecosystem = await ecosystemRepository
        .GetByIdAsync(rule.EcosystemId, cancellationToken);

        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound(
                    "Ecosystem.NotFound",
                    $"Ecosystem {rule.EcosystemId} not found. "));
        }

        if (ecosystem.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
        }

        await ruleRepository.DeleteAsync(ruleId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}