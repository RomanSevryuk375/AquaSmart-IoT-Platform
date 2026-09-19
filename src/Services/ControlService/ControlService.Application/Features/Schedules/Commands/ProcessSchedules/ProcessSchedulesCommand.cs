using BuildingBlocks.Domain.Abstractions;

namespace Control.Application.Features.Schedules.Commands.ProcessSchedules;

public sealed record ProcessSchedulesCommand(DateTime ScheduledFireTime) : ICommand;
