namespace Device.Application.DTOs.Controller;

public sealed record ControllerFilterDto
{
    public string? SearchTerm { get; init; }
    public bool? IsOnline { get; init; }
}
