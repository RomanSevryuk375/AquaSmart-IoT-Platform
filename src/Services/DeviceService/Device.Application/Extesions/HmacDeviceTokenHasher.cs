// Ignore Spelling: Hasher Hmac

using System.Security.Cryptography;
using System.Text;
using Device.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Device.Application.Extesions;

public sealed class HmacDeviceTokenHasher(IOptions<DeviceSettings> options) : IDeviceTokenHasher
{
    private readonly byte[] _secretBytes = string.IsNullOrWhiteSpace(options.Value?.TokenHmacSecret)
        ? Encoding.UTF8.GetBytes("default-insecure-m2m-token-secret-replace-in-production-min-32-chars")
        : Encoding.UTF8.GetBytes(options.Value.TokenHmacSecret);

    public string GenerateRawToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return "ak_" + Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    public string ComputeHash(string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);
        byte[] tokenBytes = Encoding.UTF8.GetBytes(rawToken);
        byte[] hashBytes = HMACSHA256.HashData(_secretBytes, tokenBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public bool Verify(string rawToken, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(rawToken) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        if (!storedHash.StartsWith("$2", StringComparison.Ordinal))
        {
            string candidateHash = ComputeHash(rawToken);
            byte[] candidateBytes = Encoding.UTF8.GetBytes(candidateHash);
            byte[] storedBytes = Encoding.UTF8.GetBytes(storedHash);

            return CryptographicOperations.FixedTimeEquals(candidateBytes, storedBytes);
        }

        return BCrypt.Net.BCrypt.EnhancedVerify(rawToken, storedHash);
    }
}
