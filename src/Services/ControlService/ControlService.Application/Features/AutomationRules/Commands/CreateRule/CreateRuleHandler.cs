using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Interfaces;
using MassTransit;
using MediatR;

namespace Control.Application.Features.AutomationRules.Commands.CreateRule;

public sealed class CreateRuleHandler(
    ISecureService secureService,
    IAutomationRuleRepository ruleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateRuleCommand request,
        CancellationToken cancellationToken)
    {
        Result relayOwnership = await secureService.EnsureEcosystemOwnsRelayAsync(
            request.EcosystemId, request.RelayId, cancellationToken);
        if (relayOwnership.IsFailure)
        {
            return Result<Guid>.Failure(relayOwnership.Error);
        }

        Result<Domain.Entities.AutomationRule> createResult = Domain.Entities.AutomationRule.Create(
            NewId.NextGuid(), request.EcosystemId, request.Name, request.RelayId,
            request.Operator, request.Action, request.IsActive);
        if (createResult.IsFailure)
        {
            return Result<Guid>.Failure(createResult.Error);
        }

        Guid result = await ruleRepository.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result);
    }
}
