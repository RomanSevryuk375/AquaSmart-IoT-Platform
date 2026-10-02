namespace Device.Application.Interfaces;


public interface IDeviceTokenHasher
{
    public string GenerateRawToken();

    public string ComputeHash(string rawToken);

    public bool Verify(string rawToken, string storedHash);
}
