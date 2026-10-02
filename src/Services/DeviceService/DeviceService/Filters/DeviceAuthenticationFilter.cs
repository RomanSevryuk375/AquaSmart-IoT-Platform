// Ignore Spelling: Hmac

using BuildingBlocks.Presentation.Constants;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace Device.API.Filters;

public class DeviceAuthenticationFilter(
    ILogger<DeviceAuthenticationFilter> logger,
    IControllerRepository controllerRepository,
    IRelayCommandsRepository commandsRepository,
    IDeviceTokenHasher tokenHasher,
    IFusionCache fusionCache) : IEndpointFilter
{
    public const string HttpContextItemKey = "AuthenticatedController";
    private static readonly FusionCacheEntryOptions _cacheOptions = new()
    {
        Duration = TimeSpan.FromMinutes(5)
    };

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        HttpContext httpContext = context.HttpContext;
        HttpRequest request = httpContext.Request;
        CancellationToken cancellationToken = httpContext.RequestAborted;

        if (!DeviceAuthenticationFilterHelpers.TryGetDeviceToken(request, out string rawDeviceToken))
        {
            logger.LogWarning(
                "Device auth failed: " +
                "Missing or empty {Header} header.",
                ApiConstants.Headers.DeviceToken);

            return Results.Unauthorized();
        }

        ControllerAuthCacheItem? controller = await ResolveControllerAuthDataAsync(
            httpContext, cancellationToken);
        if (controller is null)
        {
            logger.LogWarning(
                "Device auth failed: " +
                "Controller could not be identified from request context.");

            return Results.NotFound(new { message = "Controller not found." });
        }

        if (!tokenHasher.Verify(rawDeviceToken, controller.DeviceTokenHash))
        {
            logger.LogWarning(
                "Device auth failed: " +
                "Invalid device token for Controller {ControllerId} (MAC: {Mac}).",
                controller.Id,
                controller.MacAddress);

            return Results.Unauthorized();
        }

        if (DeviceAuthenticationFilterHelpers.TryGetMacAddress(httpContext, out string requestMac) &&
            !string.Equals(controller.MacAddress, requestMac, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "Device auth failed: MAC address mismatch for Controller {ControllerId}. Expected: {ExpectedMac}, Received: {ReceivedMac}.",
                controller.Id,
                controller.MacAddress,
                requestMac);

            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        httpContext.Items[HttpContextItemKey] = controller;

        return await next(context);
    }

    private async Task<ControllerAuthCacheItem?> ResolveControllerAuthDataAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (DeviceAuthenticationFilterHelpers.TryGetMacAddress(context, out string macAddress))
        {
            return await GetControllerByMacAdddressAsync(macAddress, cancellationToken);
        }

        RouteValueDictionary routeValues = context.GetRouteData().Values;

        if (DeviceAuthenticationFilterHelpers.TryGetControllerId(routeValues, out Guid controllerId))
        {
            return await GetControllerByControllerIdAsync(controllerId, cancellationToken);
        }

        if (DeviceAuthenticationFilterHelpers.TryGetGuid(routeValues, "commandId", out Guid commandId))
        {
            RelayCommand? command = await commandsRepository.GetByIdAsync(commandId, cancellationToken);
            if (command is not null)
            {
                return await TryGetControllerByCommandIdAsync(command, cancellationToken);
            }
        }

        return null;
    }

    private async Task<ControllerAuthCacheItem?> TryGetControllerByCommandIdAsync(RelayCommand command, CancellationToken cancellationToken)
    {
        string cacheKey = $"controller:id:{command.ControllerId}";
        return await fusionCache.GetOrSetAsync<ControllerAuthCacheItem?>(
            cacheKey,
            async (ctx, ct) =>
            {
                Domain.Entities.Controller? c = await controllerRepository.GetByIdAsync(command.ControllerId, ct);
                return c is not null
                    ? new ControllerAuthCacheItem(c.Id, c.MacAddress.Value, c.DeviceTokenHash)
                    : null;
            },
            _cacheOptions,
            cancellationToken);
    }

    private async Task<ControllerAuthCacheItem?> GetControllerByControllerIdAsync(Guid controllerId, CancellationToken cancellationToken)
    {
        string cacheKey = $"controller:id:{controllerId}";
        return await fusionCache.GetOrSetAsync<ControllerAuthCacheItem?>(
            cacheKey,
            async (ctx, ct) =>
            {
                Domain.Entities.Controller? c = await controllerRepository.GetByIdAsync(controllerId, ct);
                return c is not null
                    ? new ControllerAuthCacheItem(c.Id, c.MacAddress.Value, c.DeviceTokenHash)
                    : null;
            },
            _cacheOptions,
            cancellationToken);
    }

    private async Task<ControllerAuthCacheItem?> GetControllerByMacAdddressAsync(string macAddress, CancellationToken cancellationToken)
    {
        string cacheKey = $"controller:mac:{macAddress}";
        return await fusionCache.GetOrSetAsync<ControllerAuthCacheItem?>(
            cacheKey,
            async (ctx, ct) =>
            {
                Domain.Entities.Controller? c = await controllerRepository.GetByMacAddressAsync(macAddress, ct);
                return c is not null
                    ? new ControllerAuthCacheItem(c.Id, c.MacAddress.Value, c.DeviceTokenHash)
                    : null;
            },
            _cacheOptions,
            cancellationToken);
    }
}

public sealed record ControllerAuthCacheItem(Guid Id, string MacAddress, string DeviceTokenHash);
