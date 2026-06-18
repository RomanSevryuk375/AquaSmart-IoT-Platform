using Contracts.Abstractions;
using Device.Application.DTOs.Controller;

namespace Device.Application.CQRS.Controller.Command.AddController;

public sealed record AddControllerCommand 
    : ICommand<ControllerRegistredResponseDto>
{
    public string MacAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsOnline { get; init; }
}
