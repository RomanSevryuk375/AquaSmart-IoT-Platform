using System.Data;
using Dapper;
using Device.Application.Features.Controllers.Query.Shared;

namespace Device.Application.Features.Controllers.Query.GetControllerById;

internal sealed class GetControllerByIdHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetControllerByIdQuery, Result<ControllerDto>>
{
    public async Task<Result<ControllerDto>> Handle(
        GetControllerByIdQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT id, mac_address, name, is_online, last_seen_at
            FROM controllers
            WHERE id = @ControllerId
              AND user_id = @UserId
            LIMIT 1
            """;

        ControllerDto? controller = await connection.QueryFirstOrDefaultAsync<ControllerDto>(SQL,
            new { request.ControllerId });

        if (controller is null)
        {
            return Result<ControllerDto>.Failure(Error.NotFound<Controller>(
                string.Format(ErrorMessages.ControllerNotFound, request.ControllerId)));
        }

        return Result<ControllerDto>.Success(controller);
    }
}
