using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MassTransit;
using MediatR;

namespace Control.Application.Features.AutomationRules.Commands.CreateRule;

public sealed class CreateRuleHandler(IAutomationRuleRepository ruleRepository)
    : IRequestHandler<CreateRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateRuleCommand request,
        CancellationToken cancellationToken)
    {
        Result<AutomationRule> createResult = AutomationRule.Create(
            ruleId: NewId.NextGuid(), request.EcosystemId, request.Name, request.RelayId,
            request.Operator, request.Action, request.IsActive);
        if (createResult.IsFailure)
        {
            return Result<Guid>.Failure(createResult.Error);
        }

        Guid result = await ruleRepository.AddAsync(createResult.Value, cancellationToken);

        return Result<Guid>.Success(result);
    }
}
