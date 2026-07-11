
using System.Data;
using Dapper;
using Device.Application.Interfaces;

namespace Device.Application.Features.Integrations.ValidateDeviceToken;

public sealed class ValidateDeviceTokenHandler(ISqlConnectionFactory sqlConnectionFactory, IMyHasher myHasher)
    : IRequestHandler<ValidateDeviceTokenQuery, Result<ValidateDeviceTokenDto>>
{
    public async Task<Result<ValidateDeviceTokenDto>> Handle(
        ValidateDeviceTokenQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();
        const string SQL = """
            SELECT 
                id AS ControllerId, 
                user_id AS UserId, 
                device_token_hash AS DeviceTokenHash
            FROM controllers
            WHERE mac_address = @MacAddress
            LIMIT 1;
            """;

        ControllerAuthData? controllerData = await connection.QuerySingleOrDefaultAsync<ControllerAuthData>(
            SQL, new { request.MacAddress });

        if (controllerData is null ||
            !myHasher.Verify(request.RawDeviceToken, controllerData.DeviceTokenHash))
        {
            return Result<ValidateDeviceTokenDto>.Success(new ValidateDeviceTokenDto
            {
                IsValid = false,
                ControllerId = Guid.Empty,
                UserId = Guid.Empty,
            });
        }

        return Result<ValidateDeviceTokenDto>.Success(new ValidateDeviceTokenDto
        {
            IsValid = true,
            ControllerId = controllerData.ControllerId,
            UserId = controllerData.UserId,
        });
    }

    private sealed record ControllerAuthData
    {
        public Guid ControllerId { get; init; }
        public Guid UserId { get; init; }
        public string DeviceTokenHash { get; init; } = string.Empty;
    }
}
