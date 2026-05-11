namespace Device.Application.DTOs.Controller;

public sealed record ControllerRequestDto
{
    public string MacAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsOnline { get; init; }
}

