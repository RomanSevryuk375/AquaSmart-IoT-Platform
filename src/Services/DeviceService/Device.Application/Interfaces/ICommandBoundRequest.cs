namespace Device.Application.Interfaces;

internal interface ICommandBoundRequest
{
    public Guid CommandId { get; }
    public string DeviceToken { get; }
}
