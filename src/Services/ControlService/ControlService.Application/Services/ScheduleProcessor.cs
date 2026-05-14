using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Interfaces;
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
        var specifiction = new ActiveScheduleSpecification();
        var actionExpireAt = DateTime.UtcNow.AddMinutes(5);
        var schedules = await scheduleRepository.GetAllAsync(
            specifiction,
            null,
            null,
            cancellationToken);

        if (schedules is null)
        {
            return Result.Success();
        }

        var roundedTime = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                DateTime.UtcNow.Day,
                DateTime.UtcNow.Hour,
                DateTime.UtcNow.Minute,
                0, DateTimeKind.Utc);

        foreach (var schedule in schedules)
        {
            var cron = CronExpression
                .Parse(schedule.CronExpression, CronFormat.Standard)
                .GetNextOccurrence(roundedTime.AddTicks(-1));

            if (roundedTime == cron)
            {
                var relay = await relayRepository.GetByIdAsync(
                    schedule.RelayId, cancellationToken);

                if (relay is null || relay.IsManual)
                {
                    continue;
                }

                if (relay.IsActive is not true)
                {
                    relay.SetState(true, actionExpireAt);

                    await relayRepository.UpdateAsync(relay, cancellationToken);
                }

                await messageScheduler.SchedulePublish(
                    DateTime.UtcNow.AddMinutes(schedule.DurationMin),
                    new ChangeRelayStateEvent
                    {
                        RelayId = relay.Id,
                        Action = RuleActionEnum.SwitchOff,
                    }, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
