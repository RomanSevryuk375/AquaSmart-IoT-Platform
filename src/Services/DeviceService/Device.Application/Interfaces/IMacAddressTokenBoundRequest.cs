namespace Device.Application.Interfaces;

public interface IMacAddressTokenBoundRequest
{
    public string DeviceToken { get; }
    public string MacAddress { get; }
}
