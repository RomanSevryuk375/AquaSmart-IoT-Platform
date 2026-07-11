// Ignore Spelling: Dto

namespace Telemetry.Application.DTOs;

public sealed record ValidateResponseDto
{
    public Guid ControllerId { get; init; }
    public Guid UserId { get; init; }
}
