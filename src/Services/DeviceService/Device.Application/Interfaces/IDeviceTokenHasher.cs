namespace Device.Application.Interfaces;

/// <summary>
/// Fast HMAC-SHA256 hasher for M2M (controller) device tokens.
/// Intentionally separate from IMyHasher (BCrypt) used for user passwords.
/// </summary>
public interface IDeviceTokenHasher
{
    /// <summary>Generates a cryptographically random raw token with "ak_" prefix.</summary>
    string GenerateRawToken();

    /// <summary>Computes HMAC-SHA256 of the raw token using the service secret.</summary>
    string ComputeHash(string rawToken);

    /// <summary>
    /// Verifies a raw token against a stored hash.
    /// Supports BCrypt hashes for backward compatibility (strategy B: dev stand only).
    /// </summary>
    bool Verify(string rawToken, string storedHash);
}
