using System.Data;
using Contracts.Results;
using Control.Domain.Interfaces;
using Dapper;
using MediatR;

namespace Control.Application.Features.Ecosystems.Queries.GetAllEcosystems;

internal class GetAllEcosystemsHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllEcosystemsQuery, Result<IReadOnlyList<EcosystemDto>>>
{
    public async Task<Result<IReadOnlyList<EcosystemDto>>> Handle(
        GetAllEcosystemsQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT
              id,
              type,
              name,
              volume,
              controller_id,
              created_at
            FROM ecosystems
            WHERE user_id = @UserId
              AND (@Name IS NULL OR @Name = '' OR name ILIKE '%' || @Name || '%')
              AND (@ControllerId IS NULL OR controller_id = @ControllerId)
              AND (@Type IS NULL OR type = @Type)
            ORDER BY created_at DESC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<EcosystemDto> ecosystems = await connection.QueryAsync<EcosystemDto>(SQL, new
        {
            request.UserId,
            request.Name,
            request.ControllerId,
            request.Type,
            request.Skip,
            request.Take
        });

        return Result<IReadOnlyList<EcosystemDto>>.Success(ecosystems.ToList().AsReadOnly());
    }
}
