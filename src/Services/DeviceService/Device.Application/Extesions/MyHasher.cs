using Device.Application.Interfaces;

namespace Device.Application.Extesions;

public sealed class MyHasher : IMyHasher
{
    public string Generate(string token) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(token);

    public bool Verify(string token, string tokenHash) =>
        BCrypt.Net.BCrypt.EnhancedVerify(token, tokenHash);
}
