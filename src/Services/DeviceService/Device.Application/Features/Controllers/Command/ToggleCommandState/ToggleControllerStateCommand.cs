using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Controllers.Command.ToggleCommandState;

internal sealed record ToggleControllerStateCommand
    : ICommand<bool>, IControllerBoundRequest
{
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
}
