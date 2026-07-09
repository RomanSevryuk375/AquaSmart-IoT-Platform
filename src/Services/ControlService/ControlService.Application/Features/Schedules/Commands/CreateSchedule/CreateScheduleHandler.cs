using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Schedules.Commands.CreateSchedule;

public sealed class CreateScheduleHandler(
    IRelayRepository relayRepository,
    IScheduleRepository scheduleRepository,
    ICronValidator cronValidator) : IRequestHandler<CreateScheduleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        Relay? relay = await relayRepository.GetByIdAsync(request.RelayId, cancellationToken);
        if (relay is null || relay.EcosystemId != request.EcosystemId)
        {
            return Result<Guid>.Failure(Error.Validation<Schedule>(
                "Relay not found or belongs to another ecosystem."));
        }

        Result<Schedule> scheduleResult = Schedule.Create(
            Guid.NewGuid(), request.EcosystemId, request.RelayId,
            request.CronExpression, cronValidator, request.DurationMin,
            request.IsFadeMode, request.IsEnabled);

        if (scheduleResult.IsFailure)
        {
            return Result<Guid>.Failure(scheduleResult.Error);
        }

        await scheduleRepository.AddAsync(scheduleResult.Value, cancellationToken);

        return Result<Guid>.Success(scheduleResult.Value.Id);
    }
}
