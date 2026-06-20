namespace Device.Application.Interfaces;

internal interface IDeviceBoundRequest
{
    public Guid ControllerId { get; }
    public string DeviceToken { get; }
}
