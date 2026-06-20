using Contracts.Abstractions;

namespace Device.Application.Features.Controllers.Command.DeleteController;

internal sealed record DeleteControllerCommand : ICommand
{
    public Guid SensorId { get; init; }
    public Guid ControllerId { get; init; }
}
