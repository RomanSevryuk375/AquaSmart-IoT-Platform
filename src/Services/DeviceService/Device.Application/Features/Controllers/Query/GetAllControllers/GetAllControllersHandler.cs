using System.Data;
using Dapper;
using Device.Application.Features.Controllers.Query.Shared;

namespace Device.Application.Features.Controllers.Query.GetAllControllers;

internal class GetAllControllersHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllControllersQuery, Result<IReadOnlyList<ControllerDto>>>
{
    public async Task<Result<IReadOnlyList<ControllerDto>>> Handle(
        GetAllControllersQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT id, mac_address, name, is_online, last_seen_at
            FROM controllers
            WHERE user_id = @UserId
              AND (@SearchTerm IS NULL OR name ILIKE '%' || @SearchTerm || '%'
                                       OR mac_address ILIKE '%' || @SearchTerm || '%')
              AND (@IsOnline IS NULL OR is_online = @IsOnline)
              AND user_id = @UserId
            ORDER BY created_at DESC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<ControllerDto> controllerList = await connection.QueryAsync<ControllerDto>(SQL,
            new
            {
                request.UserId,
                request.SearchTerm,
                request.IsOnline,
                request.Skip,
                request.Take,
            });

        return Result<IReadOnlyList<ControllerDto>>.Success(controllerList.ToList().AsReadOnly());
    }
}
