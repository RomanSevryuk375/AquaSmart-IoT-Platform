namespace Device.Application.DTOs.Controller;

public sealed record ControllerUpdateRequestDto
{
    public string MacAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
