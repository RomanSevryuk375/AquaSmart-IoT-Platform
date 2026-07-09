namespace Device.Application.Interfaces;

public interface ICommandBoundRequest
{
    public Guid CommandId { get; }
    public string DeviceToken { get; }
}
