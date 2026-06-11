using Contracts.Abstractions;
using Contracts.Enums;


namespace Control.Application.CQRS.Ecosystem.Commands.CreateEcosystem;

public sealed record CreateEcosystemCommand : ICommand<Guid>
{
    public EcosystemTypeEnum Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public double? Volume { get; init; }
    public Guid ControllerId { get; init; }
    public Guid UserId { get; init; }
}
