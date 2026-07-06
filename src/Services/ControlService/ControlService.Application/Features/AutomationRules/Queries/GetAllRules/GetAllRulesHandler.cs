using System.Data;
using Contracts.Enums;
using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Domain.Interfaces;
using Control.Domain.ValueObjects;
using Dapper;
using MediatR;

namespace Control.Application.Features.AutomationRules.Queries.GetAllRules;

internal class GetAllRulesHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllRulesQuery, Result<IReadOnlyList<AutomationRuleDto>>>
{
    public async Task<Result<IReadOnlyList<AutomationRuleDto>>> Handle(
        GetAllRulesQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string RulesSql = """
            SELECT
              r.id,
              r.ecosystem_id,
              r.relay_id,
              r.name,
              r.operator,
              r.action,
              r.is_active,
              r.created_at
            FROM automation_rules r
            INNER JOIN ecosystems e ON r.ecosystem_id = e.id
            WHERE r.ecosystem_id = @EcosystemId
              AND e.user_id = @UserId
              AND (@RelayId IS NULL OR r.relay_id = @RelayId)
              AND (@Action IS NULL OR r.action = @Action)
              AND (@Operator IS NULL OR r.operator = @Operator)
            ORDER BY r.created_at DESC
            LIMIT @Take OFFSET @Skip
            """;

        var rules = (await connection.QueryAsync<AutomationRuleDto>(RulesSql, new
        {
            request.EcosystemId,
            request.UserId,
            request.RelayId,
            request.Action,
            request.Operator,
            request.Skip,
            request.Take
        })).ToList();

        if (rules.Count == 0)
        {
            return Result<IReadOnlyList<AutomationRuleDto>>.Success(
                new List<AutomationRuleDto>().AsReadOnly());
        }

        var ruleIds = rules.Select(r => r.Id).ToList();

        const string ConditionsSql = """
            SELECT id, sensor_id, condition, condition_threshold, automation_rule_id
            FROM rule_conditions
            WHERE automation_rule_id = ANY(@ruleIds)
            """;

        IEnumerable<RuleConditionFlat> conditionsFlat = await connection.QueryAsync<RuleConditionFlat>(
            ConditionsSql, new { ruleIds });

        var conditionsByRuleId = conditionsFlat
            .GroupBy(c => c.AutomationRuleId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(c =>
                {
                    var parsedThreshold = ConditionThreshold.Parse(c.ConditionThreshold);
                    return new RuleConditionResponseDto
                    {
                        Id = c.Id,
                        SensorId = c.SensorId,
                        Condition = c.Condition,
                        Threshold = parsedThreshold.Threshold,
                        Hysteresis = parsedThreshold.Hysteresis
                    };
                }).ToList()
            );

        var enrichedRules = rules.Select(r =>
        {
            if (conditionsByRuleId.TryGetValue(r.Id, out List<RuleConditionResponseDto>? conds))
            {
                return r with { Conditions = conds.AsReadOnly() };
            }
            return r;
        }).ToList();

        return Result<IReadOnlyList<AutomationRuleDto>>.Success(enrichedRules.AsReadOnly());
    }
}

internal sealed record RuleConditionFlat
{
    public Guid Id { get; init; }
    public Guid SensorId { get; init; }
    public Condition Condition { get; init; }
    public string ConditionThreshold { get; init; } = string.Empty;
    public Guid AutomationRuleId { get; init; }
}
