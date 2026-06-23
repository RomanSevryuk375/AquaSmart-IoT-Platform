namespace Device.Application.Features.Controllers.Command.AddController;

public sealed record ControllerRegisteredResponse
{
    public Guid ControllerId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
}
