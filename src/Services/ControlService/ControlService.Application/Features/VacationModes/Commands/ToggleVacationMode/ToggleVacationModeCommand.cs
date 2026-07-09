using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.VacationModes.Commands.ToggleVacationMode;

public sealed record ToggleVacationModeCommand
    : ICommand, IVacationModeBoundRequest
{
    public Guid VacationModeId { get; init; }
}
