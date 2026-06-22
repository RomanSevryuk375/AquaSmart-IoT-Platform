namespace Device.Application.Interfaces;

internal interface IMacAddressTokenBoudRequest
{
    public string DeviceToken { get; }
    public string MacAddress { get; }
}
