using Contracts.Abstractions;

namespace Device.Application.Features.Controllers.Command.ToggleCommandState;

internal sealed record ToggleControllerStateCommand
    : ICommand<bool>
{
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
}
