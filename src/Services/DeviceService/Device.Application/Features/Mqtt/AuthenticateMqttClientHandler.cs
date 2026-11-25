using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Domain.Results;
using Device.Application.Constants;
using Device.Application.Extesions;
using Device.Application.Interfaces;
using Device.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Device.Application.Features.Mqtt;

public sealed class AuthenticateMqttClientHandler(
    IControllerRepository controllerRepository,
    IDeviceTokenHasher tokenHasher,
    IFusionCache cache,
    IOptions<MqttOptions> mqttOptions,
    ILogger<AuthenticateMqttClientHandler> logger)
    : IRequestHandler<AuthenticateMqttClientQuery, Result<MqttAuthResult>>
{
    private const string DeviceClientIdPrefix = "aqua-esp32-";
    private static readonly FusionCacheEntryOptions _cacheOptions = new()
    {
        Duration = TimeSpan.FromMinutes(5)
    };

    public async Task<Result<MqttAuthResult>> Handle(
        AuthenticateMqttClientQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.ClientId))
        {
            logger.LogWarning("MQTT auth denied: missing username, password, or clientId.");
            return Result<MqttAuthResult>.Success(MqttAuthResult.Deny());
        }

        MqttOptions workerOptions = mqttOptions.Value;
        if (!string.IsNullOrWhiteSpace(workerOptions.Username) &&
            !string.IsNullOrWhiteSpace(workerOptions.Password) &&
            FixedTimeEquals(request.Username, workerOptions.Username) &&
            FixedTimeEquals(request.Password, workerOptions.Password))
        {
            return Result<MqttAuthResult>.Success(MqttAuthResult.AllowSuperuser());
        }

        Result<MacAddress> macResult = MacAddress.Create(request.Username);
        if (macResult.IsFailure)
        {
            logger.LogWarning("MQTT auth denied: invalid MAC username {Username}.", request.Username);
            return Result<MqttAuthResult>.Success(MqttAuthResult.Deny());
        }

        string canonicalMac = macResult.Value.Value;
        string compactMac = macResult.Value.ToCompactHex();
        string expectedClientId = $"{DeviceClientIdPrefix}{compactMac}";

        if (!string.Equals(request.ClientId.Trim(), expectedClientId, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "MQTT auth denied: ClientId mismatch for MAC {Mac}. Expected {Expected}, received {Actual}.",
                canonicalMac,
                expectedClientId,
                request.ClientId);
            return Result<MqttAuthResult>.Success(MqttAuthResult.Deny());
        }

        ControllerMqttAuthCacheItem? authItem = await cache.GetOrSetAsync<ControllerMqttAuthCacheItem?>(
            CacheKeys.ControllerMqttAuth(canonicalMac),
            async (_, ct) =>
            {
                Controller? controller = await controllerRepository.GetByMacAddressAsync(canonicalMac, ct);
                return controller is null
                    ? null
                    : new ControllerMqttAuthCacheItem(controller.Id, controller.DeviceTokenHash);
            },
            _cacheOptions,
            cancellationToken);

        if (authItem is null)
        {
            logger.LogWarning("MQTT auth denied: controller with MAC {Mac} not found.", canonicalMac);
            return Result<MqttAuthResult>.Success(MqttAuthResult.Deny());
        }

        if (!tokenHasher.Verify(request.Password, authItem.DeviceTokenHash))
        {
            logger.LogWarning(
                "MQTT auth denied: invalid device token for Controller {ControllerId} (MAC: {Mac}).",
                authItem.ControllerId,
                canonicalMac);
            return Result<MqttAuthResult>.Success(MqttAuthResult.Deny());
        }

        return Result<MqttAuthResult>.Success(MqttAuthResult.AllowDevice());
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        byte[] leftHash = SHA256.HashData(Encoding.UTF8.GetBytes(left));
        byte[] rightHash = SHA256.HashData(Encoding.UTF8.GetBytes(right));
        return CryptographicOperations.FixedTimeEquals(leftHash, rightHash);
    }

    public sealed record ControllerMqttAuthCacheItem(Guid ControllerId, string DeviceTokenHash);
}
