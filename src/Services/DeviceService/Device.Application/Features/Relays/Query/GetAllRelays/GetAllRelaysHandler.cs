using System.Data;
using Dapper;
using Device.Application.Features.Relays.Query.Shared;

namespace Device.Application.Features.Relays.Query.GetAllRelays;

internal sealed class GetAllRelaysHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllRelaysQuery, Result<IReadOnlyList<RelayDto>>>
{
    public async Task<Result<IReadOnlyList<RelayDto>>> Handle(
        GetAllRelaysQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT 
                id, controller_id, power_sensor_id, name, connection_protocol, 
                connection_address, is_normally_open, purpose, is_active, 
                is_manual, created_at
            FROM relays
            WHERE user_id = @UserId
              AND (@ControllerId IS NULL OR controller_id = @ControllerId)
              AND (@Purpose IS NULL OR purpose = @Purpose)
              AND (@IsActive IS NULL OR is_active = @IsActive)
              AND (@IsManual IS NULL OR is_manual = @IsManual)
            ORDER BY created_at DESC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<RelayDto> relays = await connection.QueryAsync<RelayDto>(SQL,
            new
            {
                request.UserId,
                request.ControllerId,
                request.Purpose,
                request.IsActive,
                request.IsManual,
                request.Take,
                request.Skip
            });

        return Result<IReadOnlyList<RelayDto>>.Success(relays.AsList().AsReadOnly());
    }
}
