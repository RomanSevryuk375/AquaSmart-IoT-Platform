// Ignore Spelling: Json

using System.Data;
using System.Text.Json;
using Contracts.Results;
using Dapper;
using MediatR;
using Notification.Application.Features.MaintenanceLogs.Queries.Shared;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.MaintenanceLogs.Queries.GetAllMaintenanceLogs;

public sealed class GetAllMaintenanceLogsHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllMaintenanceLogsQuery, Result<IReadOnlyList<MaintenanceLogDto>>>
{
    public async Task<Result<IReadOnlyList<MaintenanceLogDto>>> Handle(
        GetAllMaintenanceLogsQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT 
                id AS Id, 
                user_id AS UserId, 
                ecosystem_id AS EcosystemId, 
                action_date AS ActionDate, 
                metrics::text AS MetricsJson, 
                notes AS Notes, 
                created_at AS CreatedAt
            FROM maintenance_logs
            WHERE user_id = @UserId
              AND (@EcosystemId IS NULL OR ecosystem_id = @EcosystemId)
              AND (@ActionDateFrom IS NULL OR action_date >= @ActionDateFrom)
              AND (@ActionDateTo IS NULL OR action_date <= @ActionDateTo)
            ORDER BY action_date DESC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<MaintenanceLogFlatDto> flatLogs = await connection.QueryAsync<MaintenanceLogFlatDto>(
            SQL, request);

        var result = flatLogs.Select(x => new MaintenanceLogDto
        {
            Id = x.Id,
            UserId = x.UserId,
            EcosystemId = x.EcosystemId,
            ActionDate = x.ActionDate,
            Metrics = string.IsNullOrWhiteSpace(x.MetricsJson)
                ? []
                : JsonSerializer.Deserialize<Dictionary<string, double>>(x.MetricsJson) ?? [],
            Notes = x.Notes,
            CreatedAt = x.CreatedAt
        }).ToList();

        return Result<IReadOnlyList<MaintenanceLogDto>>.Success(result.AsReadOnly());
    }
}

internal sealed record MaintenanceLogFlatDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid EcosystemId { get; init; }
    public DateTime ActionDate { get; init; }
    public string MetricsJson { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
