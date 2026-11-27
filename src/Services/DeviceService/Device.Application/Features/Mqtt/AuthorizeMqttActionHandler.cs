using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using Device.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Application.Features.Mqtt;

public sealed class AuthorizeMqttActionHandler(
    IOptions<MqttOptions> mqttOptions,
    ILogger<AuthorizeMqttActionHandler> logger)
    : IRequestHandler<AuthorizeMqttActionQuery, Result<MqttAclResult>>
{
    public Task<Result<MqttAclResult>> Handle(
        AuthorizeMqttActionQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Topic) ||
            string.IsNullOrWhiteSpace(request.Action))
        {
            logger.LogWarning("MQTT ACL denied: missing username, topic, or action.");
            return Task.FromResult(Result<MqttAclResult>.Success(MqttAclResult.Deny()));
        }

        MqttOptions workerOptions = mqttOptions.Value;
        if (!string.IsNullOrWhiteSpace(workerOptions.Username) &&
            FixedTimeEquals(request.Username, workerOptions.Username))
        {
            return Task.FromResult(Result<MqttAclResult>.Success(MqttAclResult.AllowAction()));
        }

        string topic = request.Topic.Trim();
        if (topic.Contains('+', StringComparison.Ordinal) ||
            topic.Contains('#', StringComparison.Ordinal))
        {
            logger.LogWarning(
                "MQTT ACL denied: wildcards are forbidden for device {Username} on topic {Topic}.",
                request.Username,
                topic);
            return Task.FromResult(Result<MqttAclResult>.Success(MqttAclResult.Deny()));
        }

        Result<string> macResult = MacAddress.NormalizeToCompactHex(request.Username);
        if (macResult.IsFailure)
        {
            logger.LogWarning("MQTT ACL denied: invalid MAC username {Username}.", request.Username);
            return Task.FromResult(Result<MqttAclResult>.Success(MqttAclResult.Deny()));
        }

        string compactMac = macResult.Value;
        string action = request.Action.Trim().ToLowerInvariant();
        string topicPrefix = $"aquasmart/devices/{compactMac}/";

        bool allowed = action switch
        {
            "publish" =>
                string.Equals(topic, $"{topicPrefix}telemetry", StringComparison.Ordinal) ||
                string.Equals(topic, $"{topicPrefix}commands/ack", StringComparison.Ordinal) ||
                string.Equals(topic, $"{topicPrefix}relays/state", StringComparison.Ordinal) ||
                string.Equals(topic, $"{topicPrefix}status", StringComparison.Ordinal),
            "subscribe" =>
                string.Equals(topic, $"{topicPrefix}commands", StringComparison.Ordinal) ||
                string.Equals(topic, $"{topicPrefix}config/notify", StringComparison.Ordinal),
            _ => false
        };

        if (!allowed)
        {
            logger.LogWarning(
                "MQTT ACL denied for MAC {Mac}: action {Action} on topic {Topic} is not allowed.",
                compactMac,
                action,
                topic);
        }

        return Task.FromResult(Result<MqttAclResult>.Success(
            allowed ? MqttAclResult.AllowAction() : MqttAclResult.Deny()));
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        byte[] leftHash = SHA256.HashData(Encoding.UTF8.GetBytes(left));
        byte[] rightHash = SHA256.HashData(Encoding.UTF8.GetBytes(right));
        return CryptographicOperations.FixedTimeEquals(leftHash, rightHash);
    }
}
