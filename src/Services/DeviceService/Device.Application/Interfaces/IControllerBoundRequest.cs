namespace Device.Application.Interfaces;

internal interface IControllerBoundRequest
{
    public Guid ControllerId { get; }
}
