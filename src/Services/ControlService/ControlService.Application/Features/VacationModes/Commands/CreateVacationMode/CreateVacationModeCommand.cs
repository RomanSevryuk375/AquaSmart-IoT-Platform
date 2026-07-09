using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.VacationModes.Commands.CreateVacationMode;

public sealed record CreateVacationModeCommand
    : ICommand<Guid>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsActive { get; init; }
    public double CalculatedFeed { get; init; }
}
