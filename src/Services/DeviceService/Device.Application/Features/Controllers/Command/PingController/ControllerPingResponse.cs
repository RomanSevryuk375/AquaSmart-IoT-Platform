namespace Device.Application.Features.Controllers.Command.PingController;

public sealed record ControllerPingResponse
{
    public DateTime ServerTimeUtc { get; init; } = DateTime.UtcNow;
}
