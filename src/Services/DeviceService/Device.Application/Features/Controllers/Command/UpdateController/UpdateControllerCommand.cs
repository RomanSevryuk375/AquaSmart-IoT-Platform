using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Controllers.Command.UpdateController;

public sealed record UpdateControllerCommand
    : ICommand, IControllerBoundRequest
{
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
