using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Controllers.Command.PingController;

public sealed record PingControllerCommand
    : ICommand<ControllerPingResponse>, IDeviceBoundRequest
{
    public Guid ControllerId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
}
