using Contracts.Abstractions;

namespace Device.Application.Features.Controllers.Command.UpdateController;

internal sealed class UpdateControllerCommand : ICommand
{
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
