using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Presentation.Constants;
using Device.Application.Extesions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Device.API.Filters;

public sealed class InternalApiKeyFilter(
    IOptions<MqttBrokerOptions> brokerOptions,
    ILogger<InternalApiKeyFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        string expectedSecret = brokerOptions.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(expectedSecret))
        {
            logger.LogWarning("Internal API key check failed: WebhookSecret is not configured.");
            return Results.Unauthorized();
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiConstants.Headers.InternalApiKey, out StringValues headerValues) ||
            string.IsNullOrWhiteSpace(headerValues))
        {
            logger.LogWarning(
                "Internal API key check failed: missing {Header} header.",
                ApiConstants.Headers.InternalApiKey);
            return Results.Unauthorized();
        }

        string providedKey = headerValues.ToString().Trim();
        byte[] providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(providedKey));
        byte[] expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expectedSecret));

        if (!CryptographicOperations.FixedTimeEquals(providedHash, expectedHash))
        {
            logger.LogWarning("Internal API key check failed: invalid key.");
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
