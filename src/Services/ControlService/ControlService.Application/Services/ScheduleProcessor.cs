using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Control.Domain.Specifications;
using Cronos;
using MassTransit;

namespace Control.Application.Services;

public sealed class ScheduleProcessor(
    IScheduleRepository scheduleRepository,
    IRelayRepository relayRepository,
    IMessageScheduler messageScheduler,
    IUnitOfWork unitOfWork) : IScheduleProcessor
{
    public async Task<Result> ProcessAsync(CancellationToken cancellationToken)
    {
        var specification = new ActiveScheduleSpecification();
        IReadOnlyList<ScheduleEntity> schedules = await scheduleRepository.GetAllAsync(
            specification, cancellationToken);

        if (schedules == null || !schedules.Any())
        {
            return Result.Success();
        }

        var roundedTime = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0, DateTimeKind.Utc);

        var triggeredSchedules = schedules.Where(s =>
        {
            var cron = CronExpression.Parse(s.CronExpression, CronFormat.Standard);
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

        foreach (ScheduleEntity? schedule in triggeredSchedules)
        {
            if (!relaysCache.TryGetValue(schedule.RelayId, out RelayEntity? relay) || relay.IsManual)
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
