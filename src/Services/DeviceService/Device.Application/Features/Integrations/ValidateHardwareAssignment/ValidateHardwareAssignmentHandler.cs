using System.Data;
using Dapper;

namespace Device.Application.Features.Integrations.ValidateHardwareAssignment;

public sealed class ValidateHardwareAssignmentHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<ValidateHardwareAssignmentQuery, Result<ValidateHardwareAssignmentDto>>
{
    public async Task<Result<ValidateHardwareAssignmentDto>> Handle(
        ValidateHardwareAssignmentQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT EXISTS (
                SELECT 1
                FROM sensors s
                JOIN relays r ON s.controller_id = r.controller_id
                WHERE s.id = @SensorId AND r.id = @RelayId
            );
            """;

        bool isValid = await connection.QuerySingleAsync<bool>(SQL, new
        {
            request.SensorId,
            request.RelayId
        });

        return Result<ValidateHardwareAssignmentDto>.Success(
            new ValidateHardwareAssignmentDto { IsValid = isValid });
    }
}
