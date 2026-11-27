using System.Data;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using Dapper;
using Device.Application.Constants;
using Device.Application.Features.Integrations.ValidateDeviceToken;
using Device.Application.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace Device.Application.Features.Integrations.ValidateDeviceTokenByControllerId;

public sealed class ValidateDeviceTokenByControllerIdHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IDeviceTokenHasher tokenHasher,
    IFusionCache cache)
    : IRequestHandler<ValidateDeviceTokenByControllerIdQuery, Result<ValidateDeviceTokenDto>>
{
    private static readonly FusionCacheEntryOptions _cacheOptions = new()
    {
        Duration = TimeSpan.FromMinutes(5)
    };

    public async Task<Result<ValidateDeviceTokenDto>> Handle(
        ValidateDeviceTokenByControllerIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ControllerId == Guid.Empty || string.IsNullOrWhiteSpace(request.RawDeviceToken))
        {
            return Result<ValidateDeviceTokenDto>.Success(Invalid());
        }

        ControllerTokenCacheItem? controllerData = await cache.GetOrSetAsync<ControllerTokenCacheItem?>(
            CacheKeys.ControllerTokenAuth(request.ControllerId),
            async (ctx, _) =>
            {
                using IDbConnection connection = sqlConnectionFactory.CreateConnection();
                const string Sql = """
                    SELECT
                        id AS ControllerId,
                        user_id AS UserId,
                        device_token_hash AS DeviceTokenHash
                    FROM controllers
                    WHERE id = @ControllerId
                    LIMIT 1;
                    """;

                ControllerTokenCacheItem? item = await connection.QuerySingleOrDefaultAsync<ControllerTokenCacheItem>(
                    Sql, new { request.ControllerId });
                if (item is null)
                {
                    ctx.Options.Duration = TimeSpan.Zero;
                }

                return item;
            },
            _cacheOptions,
            cancellationToken);

        if (controllerData is null ||
            !tokenHasher.Verify(request.RawDeviceToken, controllerData.DeviceTokenHash))
        {
            return Result<ValidateDeviceTokenDto>.Success(Invalid());
        }

        return Result<ValidateDeviceTokenDto>.Success(new ValidateDeviceTokenDto
        {
            IsValid = true,
            ControllerId = controllerData.ControllerId,
            UserId = controllerData.UserId,
        });
    }

    private static ValidateDeviceTokenDto Invalid() => new()
    {
        IsValid = false,
        ControllerId = Guid.Empty,
        UserId = Guid.Empty,
    };

    public sealed record ControllerTokenCacheItem
    {
        public Guid ControllerId { get; init; } = Guid.Empty;
        public Guid UserId { get; init; } = Guid.Empty;
        public string DeviceTokenHash { get; init; } = string.Empty;
    }
}
