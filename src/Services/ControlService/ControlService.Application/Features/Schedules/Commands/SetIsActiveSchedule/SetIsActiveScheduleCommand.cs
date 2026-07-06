using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.Schedules.Commands.SetIsActiveSchedule;

public sealed record SetIsActiveScheduleCommand
    : ICommand, IScheduleBoundRequest
{
    public Guid ScheduleId { get; init; }
    public bool IsActive { get; init; }
}
