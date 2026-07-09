using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.VacationModes.Commands.UpdateVacationMode;

public sealed record UpdateVacationModeCommand
    : ICommand, IVacationModeBoundRequest
{
    public Guid VacationModeId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public double CalculatedFeed { get; init; }
}
