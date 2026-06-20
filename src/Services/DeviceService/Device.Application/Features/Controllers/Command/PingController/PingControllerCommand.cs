using Contracts.Abstractions;

namespace Device.Application.Features.Controllers.Command.PingController;

internal sealed record PingControllerCommand
    : ICommand<ControllerPingResponse>
{
    public Guid ControllerId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
}
