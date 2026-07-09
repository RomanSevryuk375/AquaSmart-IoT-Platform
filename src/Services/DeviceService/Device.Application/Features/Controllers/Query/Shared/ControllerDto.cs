// Ignore Spelling: Dto

namespace Device.Application.Features.Controllers.Query.Shared;

public sealed record ControllerDto
{
    public Guid Id { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsOnline { get; init; }
    public DateTime LastSeenAt { get; init; }
}
