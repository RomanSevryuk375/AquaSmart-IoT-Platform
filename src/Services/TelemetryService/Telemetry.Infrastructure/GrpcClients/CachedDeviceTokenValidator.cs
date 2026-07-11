// Ignore Spelling: Validator

using Contracts.Results;
using Microsoft.Extensions.Caching.Memory;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.GrpcClients;

public sealed class CachedDeviceTokenValidator(
    IDeviceTokenValidator decorated,
    IMemoryCache memoryCache) : IDeviceTokenValidator
{
    public async Task<Result<ValidateResponseDto>> ValidateAsync(
        string macAddress,
        string deviceToken,
        CancellationToken cancellationToken = default)
    {
        string upperAddress = macAddress.ToUpper();
        string upperToken = deviceToken.ToUpper();

        string cacheKey = $"Key_{upperAddress}_{upperToken}";

        if (memoryCache.TryGetValue(cacheKey, out ValidateResponseDto? response))
        {
            return Result<ValidateResponseDto>.Success(response!);
        }

        Result<ValidateResponseDto> result = await decorated.ValidateAsync(
            macAddress, deviceToken, cancellationToken);
        if (result.IsSuccess)
        {
            MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            memoryCache.Set(cacheKey, result.Value, cacheOptions);
        }

        return result;
    }
}
