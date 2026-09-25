
using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Presentation.Constants;
using IdentityService.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace IdentityService.API.Filters;

public sealed class HmacValidationFilter(
    IOptions<TelegramBotOptions> options,
    ILogger<HmacValidationFilter> logger) : IEndpointFilter
{
    private static readonly TimeSpan _maxAllowedClockSkew = TimeSpan.FromMinutes(5);

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        HttpContext httpContext = context.HttpContext;
        HttpRequest request = httpContext.Request;

        if (!request.Headers.TryGetValue(ApiConstants.Headers.Timestamp, out StringValues timestampValues) ||
           !request.Headers.TryGetValue(ApiConstants.Headers.HmacSignature, out StringValues signatureValues))
        {
            logger.LogWarning("HMAC check failed: Missing 'X-Timestamp' or 'X-Signature' headers.");
            return Results.Unauthorized();
        }

        string clientSignature = signatureValues.ToString();
        string timestampStr = timestampValues.ToString();

        if (!long.TryParse(timestampStr, out long unixTimeSeconds))
        {
            logger.LogWarning("HMAC check failed: Invalid 'X-Timestamp' format.");
            return Results.Unauthorized();
        }

        DateTime requestTimeUtc = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;
        TimeSpan timeDifference = (DateTime.UtcNow - requestTimeUtc).Duration();

        if (timeDifference > _maxAllowedClockSkew)
        {
            logger.LogWarning("HMAC check failed: Clock skew {Difference}s exceeded limit of {Limit}s.",
                timeDifference.TotalSeconds, _maxAllowedClockSkew.TotalSeconds);
            return Results.Unauthorized();
        }

        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        string body = await reader.ReadToEndAsync(httpContext.RequestAborted);
        string canonicalString = $"{request.Method}\n{request.Path.Value}\n{timestampStr}\n{body}";

        string expectedSignature = ComputeHmac(canonicalString, options.Value.BotSecretKey);

        byte[] providedBytes = Encoding.UTF8.GetBytes(clientSignature);
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);

        if (!CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes))
        {
            logger.LogWarning("HMAC check failed: Signature mismatch for path {Path}.", request.Path.Value);
            return Results.Unauthorized();
        }

        request.Body.Position = 0;

        return await next(context);
    }

    private static string ComputeHmac(string data, string secretKey)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        byte[] hashBytes = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
