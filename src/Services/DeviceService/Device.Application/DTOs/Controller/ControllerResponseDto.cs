namespace Device.Application.DTOs.Controller;
public sealed record ControllerResponseDto
{
    public Guid Id { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsOnline { get; init; }
    public DateTime LastSeenAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
