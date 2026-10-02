using BuildingBlocks.Domain.Abstractions;

namespace Device.Application.Features.Controllers.Command.PingController;

public sealed record PingControllerCommand
    : ICommand<ControllerPingResponse>
{
    public Guid ControllerId { get; init; }
}
