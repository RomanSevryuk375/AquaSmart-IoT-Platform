// Ignore Spelling: Dto

using Contracts.Enums;

namespace Control.Application.Features.Ecosystems.Queries;

public sealed record EcosystemDto
{
    public Guid Id { get; init; }
    public EcosystemType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public double? Volume { get; init; }
    public Guid ControllerId { get; init; }
    public DateTime CreatedAt { get; init; }
}
