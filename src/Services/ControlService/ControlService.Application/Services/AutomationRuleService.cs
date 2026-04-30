using Contracts.Exceptions;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Control.Domain.SpecificationParams;
using Control.Domain.Specifications;

namespace Control.Application.Services;

public class AutomationRuleService(
    IAutomationRuleRepository ruleRepository,
    ISensorRepository sensorRepository,
    IRelayRepository relayRepository,
    IAquariumRepository aquariumRepository,
    IUnitOfWork unitOfWork) : IAutomationRuleService
{
    public async Task<IReadOnlyList<AutomationRuleResponseDto>> GetAllRulesAsync(
        AutomationRuleFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var specification = new AutomationRuleFilterSpecification(
            new AutomatizationRuleFilterParams
            {
                EcosystemId = filter.AquariumId,
                SensorId = filter.SensorId,
                RelayId = filter.RelayId,
                Condition = filter.Condition,
                Action = filter.Action
            });

        var rules = await ruleRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        return rules.Select(x => new AutomationRuleResponseDto
        {
            Id = x.Id,
            AquariumId = x.EcosystemId,
            SensorId = x.SensorId,
            RelayId = x.RelayId,
            Condition = x.Condition,
            Threshold = x.Threshold,
            Hysteresis = x.Hysteresis,
            Action = x.Action,
            CreatedAt = x.CreatedAt,
        }).ToList();
    }

    public async Task<AutomationRuleResponseDto> GetRuleByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"{nameof(AutomationRuleEntity)} not found");

        return new AutomationRuleResponseDto
        {
            Id = rule.Id,
            AquariumId = rule.EcosystemId,
            SensorId = rule.SensorId,
            RelayId = rule.RelayId,
            Condition = rule.Condition,
            Threshold = rule.Threshold,
            Hysteresis = rule.Hysteresis,
            Action = rule.Action,
            CreatedAt = rule.CreatedAt,
        };
    }

    public async Task<Guid> CreateRuleAsync(
        AutomationRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        var existingAquarium = await aquariumRepository
            .GetByIdAsync(request.AquariumId, cancellationToken)
            ?? throw new NotFoundException($"Aquarium {request.AquariumId} not found");

        var existingRelay = await relayRepository
            .GetByIdAsync(request.RelayId, cancellationToken)
            ?? throw new NotFoundException($"Relay {request.RelayId} not found");

        var existingSensor = await sensorRepository
            .GetByIdAsync(request.SensorId, cancellationToken)
            ?? throw new NotFoundException($"Sensor {request.SensorId} not found");

        var (rule, errors) = AutomationRuleEntity.Create(
            request.AquariumId,
            request.SensorId,
            request.RelayId,
            request.Condition,
            request.Threshold,
            request.Hysteresis,
            request.Action);

        if (rule is null)
        {
            throw new DomainValidationException(
                $"Failed to create {nameof(AutomationRuleEntity)}: {string.Join(", ", errors)}");
        }

        var result = await ruleRepository.AddAsync(rule!, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task UpdateRuleAsync(
        Guid id,
        AutomationRuleUpdateRequestDto request,
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"{nameof(AutomationRuleEntity)} not found");

        var errors = rule.Update(
            request.Condition,
            request.Threshold,
            request.Hysteresis,
            request.Action);

        if (errors is not null)
        {
            throw new DomainValidationException(
                $"Failed to update {nameof(AutomationRuleEntity)}: {string.Join(", ", errors)}");
        }

        await ruleRepository.UpdateAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRuleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await ruleRepository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}