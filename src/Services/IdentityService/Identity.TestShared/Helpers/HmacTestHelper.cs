using System.Security.Cryptography;
using System.Text;

namespace Identity.TestShared.Helpers;

public static class HmacTestHelper
{
    public const string TestBotSecretKey = "test-telegram-bot-secret-key-12345";
    public const string TestBotName = "AquaSmartTestBot";

    public static string ComputeHmac(string data, string secretKey = TestBotSecretKey)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        byte[] hashBytes = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static (string Signature, string Timestamp) GenerateHmacHeaders(
        string method,
        string path,
        string body,
        long? unixTimestamp = null,
        string secretKey = TestBotSecretKey)
    {
        long ts = unixTimestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string timestampStr = ts.ToString();
        string canonicalString = $"{method}\n{path}\n{timestampStr}\n{body}";
        string signature = ComputeHmac(canonicalString, secretKey);

        return (signature, timestampStr);
    }
}
