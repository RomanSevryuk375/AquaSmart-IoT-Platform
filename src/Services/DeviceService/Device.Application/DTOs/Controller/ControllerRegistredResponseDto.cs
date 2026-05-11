namespace Device.Application.DTOs.Controller;

public sealed record ControllerRegistredResponseDto
{
    public Guid ControllerId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
}
