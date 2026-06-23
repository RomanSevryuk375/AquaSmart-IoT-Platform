namespace Device.Application.Features.Controllers.Query.GetControllerConfig;

internal sealed record ControllerAuthInfo
{
    public Guid ControllerId { get; init; }
    public string DeviceTokenHash { get; init; } = string.Empty;
}
