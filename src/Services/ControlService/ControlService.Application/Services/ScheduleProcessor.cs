using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Control.Application.Interfaces;
using Control.Domain.Interfaces;
using Control.Domain.Specifications;
using Cronos;
using MassTransit;

namespace Control.Application.Services;

public class ScheduleProcessor(
    IScheduleRepository scheduleRepository,
    IRelayRepository relayRepository,
    IPublishEndpoint publishEndpoint,
    IMessageScheduler messageScheduler,
    IUnitOfWork unitOfWork) : IScheduleProcessor
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var specifiction = new ActiveScheduleSpecification();

        var schedules = await scheduleRepository.GetAllAsync(
            specifiction,
            null,
            null,
            cancellationToken);

        if (schedules is null)
        {
            return;
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
                    relay.SetState(true);

                    await relayRepository.UpdateAsync(relay, cancellationToken);

                    await publishEndpoint.Publish(new ChangeRelayStateCommand
                    {
                        RelayId = relay.Id,
                        Action = RuleActionEnum.SwitchOn,
                    }, cancellationToken);
                }

                await messageScheduler.SchedulePublish(
                    DateTime.UtcNow.AddMinutes(schedule.DurationMin),
                    new ChangeRelayStateCommand
                    {
                        RelayId = relay.Id,
                        Action = RuleActionEnum.SwitchOff,
                    }, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
