using Contracts.Abstractions;

namespace Device.Application.Features.Controllers.Command.AddController;

internal sealed record AddControllerCommand 
    : ICommand<ControllerRegistredResponse>
{
    public string MacAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsOnline { get; init; }
}
