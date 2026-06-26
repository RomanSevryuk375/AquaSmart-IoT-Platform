using System.Data;
using Dapper;
using Device.Application.Features.Relays.Query.Shared;

namespace Device.Application.Features.Relays.Query.GetRelayById;

internal sealed class GetRelayByIdHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetRelayByIdQuery, Result<RelayDto>>
{
    public async Task<Result<RelayDto>> Handle(
        GetRelayByIdQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQl = """
            SELECT 
                id, controller_id, power_sensor_id, name, connection_protocol, 
                connection_address, is_normally_open, purpose, is_active, 
                is_manual, created_at
            FROM relays
            WHERE id = @RelayId AND user_id = @UserId
            LIMIT 1
            """;

        RelayDto? relay = await connection.QueryFirstOrDefaultAsync<RelayDto>(SQl,
            new { request.RelayId, request.UserId });

        if (relay is null)
        {
            return Result<RelayDto>.Failure(Error.NotFound<Relay>(
                string.Format(ErrorMessages.RelayNotFoundOrAccessDenied, request.RelayId)));
        }

        return Result<RelayDto>.Success(relay);
    }
}
