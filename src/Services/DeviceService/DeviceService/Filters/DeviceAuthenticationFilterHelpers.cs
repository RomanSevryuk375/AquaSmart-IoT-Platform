using BuildingBlocks.Presentation.Constants;
using Microsoft.Extensions.Primitives;

namespace Device.API.Filters;

internal static class DeviceAuthenticationFilterHelpers
{

    public static bool TryGetControllerId(RouteValueDictionary routeValues, out Guid controllerId)
    {
        return TryGetGuid(routeValues, "controllerId", out controllerId) ||
               TryGetGuid(routeValues, "id", out controllerId);
    }

    public static bool TryGetDeviceToken(HttpRequest request, out string token)
    {
        if (request.Headers.TryGetValue(ApiConstants.Headers.DeviceToken, out StringValues values) &&
            !string.IsNullOrWhiteSpace(values))
        {
            token = values.ToString().Trim();
            return true;
        }

        token = string.Empty;
        return false;
    }

    public static bool TryGetMacAddress(HttpContext context, out string macAddress)
    {
        if (context.Request.Headers.TryGetValue(ApiConstants.Headers.MacAddress, out StringValues values) &&
            !string.IsNullOrWhiteSpace(values))
        {
            macAddress = values.ToString().Trim().ToUpperInvariant();
            return true;
        }

        macAddress = string.Empty;
        return false;
    }

    public static bool TryGetGuid(RouteValueDictionary values, string key, out Guid result)
    {
        if (values.TryGetValue(key, out object? value) &&
            Guid.TryParse(value?.ToString(), out result))
        {
            return true;
        }

        result = Guid.Empty;
        return false;
    }
}
