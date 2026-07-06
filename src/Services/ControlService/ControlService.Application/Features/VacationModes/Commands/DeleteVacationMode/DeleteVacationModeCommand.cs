using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.VacationModes.Commands.DeleteVacationMode;

public sealed record DeleteVacationModeCommand
    : ICommand, IVacationModeBoundRequest
{
    public Guid VacationModeId { get; init; }
}
