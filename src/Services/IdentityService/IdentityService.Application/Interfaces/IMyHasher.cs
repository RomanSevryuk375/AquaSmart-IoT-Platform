namespace IdentityService.Application.Interfaces;

public interface IMyHasher
{
    string Generate(string token);
    bool Verify(string token, string tokenHash);
}