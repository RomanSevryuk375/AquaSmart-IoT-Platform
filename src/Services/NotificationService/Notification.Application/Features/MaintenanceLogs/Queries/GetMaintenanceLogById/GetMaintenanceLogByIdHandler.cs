using System.Data;
using System.Text.Json;
using Contracts.Results;
using Dapper;
using MediatR;
using Notification.Application.Features.MaintenanceLogs.Queries.GetAllMaintenanceLogs;
using Notification.Application.Features.MaintenanceLogs.Queries.Shared;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.MaintenanceLogs.Queries.GetMaintenanceLogById;

public sealed class GetMaintenanceLogByIdHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetMaintenanceLogByIdQuery, Result<MaintenanceLogDto>>
{
    public async Task<Result<MaintenanceLogDto>> Handle(
        GetMaintenanceLogByIdQuery request,
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
            WHERE id = @Id 
              AND user_id = @UserId
            LIMIT 1
            """;

        MaintenanceLogFlatDto? flatDto = await connection.QuerySingleOrDefaultAsync<MaintenanceLogFlatDto>(
            SQL, request);

        if (flatDto is null)
        {
            return Result<MaintenanceLogDto>.Failure(Error.NotFound<MaintenanceLogDto>(
                $"Maintenance log {request.Id} not found."));
        }

        var dto = new MaintenanceLogDto
        {
            Id = flatDto.Id,
            UserId = flatDto.UserId,
            EcosystemId = flatDto.EcosystemId,
            ActionDate = flatDto.ActionDate,
            Metrics = string.IsNullOrWhiteSpace(flatDto.MetricsJson)
                ? []
                : JsonSerializer.Deserialize<Dictionary<string, double>>(flatDto.MetricsJson) ?? [],
            Notes = flatDto.Notes,
            CreatedAt = flatDto.CreatedAt
        };

        return Result<MaintenanceLogDto>.Success(dto);
    }
}
