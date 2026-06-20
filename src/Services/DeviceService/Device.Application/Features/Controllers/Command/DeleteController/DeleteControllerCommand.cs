using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.Controllers.Command.DeleteController;

internal sealed record DeleteControllerCommand : ICommand, IControllerBoundRequest
{
    public Guid ControllerId { get; init; }
}
