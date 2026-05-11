namespace Device.Application.DTOs.Controller;

public sealed record ControllerPingResponseDto
{
    public DateTime ServerTimeUtc { get; init; } = DateTime.UtcNow;
}
