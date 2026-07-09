using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.Schedules.Commands.DeleteSchedule;

public sealed record DeleteScheduleCommand : ICommand, IScheduleBoundRequest
{
    public Guid ScheduleId { get; init; }
}
