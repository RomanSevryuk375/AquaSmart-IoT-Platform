namespace IdentityService.Application.Interfaces;

public interface IMyHasher
{
    public string Generate(string token);
    public bool Verify(string token, string tokenHash);
}