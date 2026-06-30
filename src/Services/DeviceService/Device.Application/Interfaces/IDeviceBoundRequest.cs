namespace Device.Application.Interfaces;

public interface IDeviceBoundRequest
{
    public Guid ControllerId { get; }
    public string DeviceToken { get; }
}
