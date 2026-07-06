using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Cronos;
using MassTransit;
using MediatR;

namespace Control.Application.Features.Schedules.Commands.ProcessSchedules;

public sealed class ProcessSchedulesHandler(
    IScheduleRepository scheduleRepository,
    IRelayRepository relayRepository,
    IMessageScheduler messageScheduler) : IRequestHandler<ProcessSchedulesCommand, Result>
{
    public async Task<Result> Handle(ProcessSchedulesCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Schedule> schedules = await scheduleRepository.GetActiveSchedules(cancellationToken);

        if (schedules == null || !schedules.Any())
        {
            return Result.Success();
        }

        var roundedTime = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0, DateTimeKind.Utc);

        var triggeredSchedules = schedules.Where(s =>
        {
            var cron = CronExpression.Parse(s.CronExpression.ToString(), CronFormat.Standard);
            DateTime? nextOccurrence = cron.GetNextOccurrence(roundedTime.AddTicks(-1));

            return nextOccurrence == roundedTime;
        }).ToList();

        if (triggeredSchedules.Count == 0)
        {
            return Result.Success();
        }

        var relayIds = triggeredSchedules.Select(s => s.RelayId).Distinct().ToList();
        var relaysCache = (await relayRepository.GetManyByIds(relayIds, cancellationToken)).ToDictionary(r => r.Id);
        DateTime actionExpireAt = DateTime.UtcNow.AddMinutes(5);

        foreach (Schedule? schedule in triggeredSchedules)
        {
            if (!relaysCache.TryGetValue(schedule.RelayId, out Relay? relay) || relay.IsManual)
            {
                continue;
            }

            if (!relay.IsActive)
            {
                relay.SetState(true, actionExpireAt);
            }

            DateTime turnOffTime = DateTime.UtcNow.AddMinutes(schedule.DurationMin);

            await messageScheduler.SchedulePublish(
                turnOffTime,
                new ChangeRelayStateEvent
                {
                    ControllerId = relay.ControllerId,
                    RelayId = relay.Id,
                    TargetState = false,
                    ExpireAt = turnOffTime.AddMinutes(5)
                }, cancellationToken);
        }

        return Result.Success();
    }
}
