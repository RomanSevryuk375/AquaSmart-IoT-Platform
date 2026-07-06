// Ignore Spelling: Dto

namespace Control.Application.Features.VacationModes.Queries.Shared;

public sealed record VacationModeDto
{
    public Guid Id { get; init; }
    public Guid EcosystemId { get; init; }
    public string DateRange { get; init; } = string.Empty;
    public double CalculatedFeed { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
