using BuildingBlocks.Domain.Results;
using BuildingBlocks.IntegrationEvents.Events.Relays;
using Control.Application.Features.Schedules.Commands.ProcessSchedules;
using Control.TestShared.Constants;
using MassTransit;

namespace Control.Application.UnitTests.Features.Schedules;

public class ProcessSchedulesHandlerTests
{
    // Mock dependencies
    private readonly IScheduleRepository _scheduleRepo = Substitute.For<IScheduleRepository>();
    private readonly IRelayRepository _relayRepo = Substitute.For<IRelayRepository>();
    private readonly IMessageScheduler _messageScheduler = Substitute.For<IMessageScheduler>();
    private readonly ProcessSchedulesHandler _handler;

    // Fixed fire time: 2024-06-15 10:00:30 UTC
    // roundedTime -> 2024-06-15 10:00:00 UTC
    private static readonly DateTime FireTime = new DateTime(2024, 6, 15, 10, 0, 30, DateTimeKind.Utc);

    // Cron `0 10 * * *` fires at 10:00 UTC daily -- matches RoundedTime
    private const string MatchingCron = "0 10 * * *";

    // Cron `0 9 * * *` fires at 09:00 -- does NOT match 10:00
    private const string NonMatchingCron = "0 9 * * *";

    public ProcessSchedulesHandlerTests()
    {
        _handler = new ProcessSchedulesHandler(_scheduleRepo, _relayRepo, _messageScheduler);
    }

    private static ProcessSchedulesCommand MakeCommand() => new(FireTime);

    [Fact]
    public async Task Handle_WhenNoActiveSchedules_ReturnsSuccessWithoutPublishing()
    {
        _scheduleRepo.GetActiveSchedules(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Schedule>());

        Result result = await _handler.Handle(MakeCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _relayRepo.DidNotReceive()
            .GetManyByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
        await _messageScheduler.DidNotReceive()
            .SchedulePublish(Arg.Any<DateTime>(), Arg.Any<ChangeRelayStateEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoScheduleMatchesFireTime_ReturnsSuccessWithoutPublishing()
    {
        var schedule = new ScheduleBuilder()
            .WithCronExpression(NonMatchingCron)
            .Build();

        _scheduleRepo.GetActiveSchedules(Arg.Any<CancellationToken>())
            .Returns(new[] { schedule });

        Result result = await _handler.Handle(MakeCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _messageScheduler.DidNotReceive()
            .SchedulePublish(Arg.Any<DateTime>(), Arg.Any<ChangeRelayStateEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRelayIsManual_SkipsAndDoesNotPublish()
    {
        var relayId = ControlTestConstants.RelayId;

        var schedule = new ScheduleBuilder()
            .WithRelayId(relayId)
            .WithCronExpression(MatchingCron)
            .Build();

        var relay = new RelayBuilder()
            .WithId(relayId)
            .WithIsManual(true)
            .WithIsActive(false)
            .Build();

        _scheduleRepo.GetActiveSchedules(Arg.Any<CancellationToken>())
            .Returns(new[] { schedule });
        _relayRepo.GetManyByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { relay });

        Result result = await _handler.Handle(MakeCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _messageScheduler.DidNotReceive()
            .SchedulePublish(Arg.Any<DateTime>(), Arg.Any<ChangeRelayStateEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenScheduleMatchesAndRelayInactive_ActivatesRelayAndSchedulesTurnOff()
    {
        var relayId = ControlTestConstants.RelayId;
        const double durationMin = 30.0;

        var schedule = new ScheduleBuilder()
            .WithRelayId(relayId)
            .WithCronExpression(MatchingCron)
            .WithDurationMin(durationMin)
            .Build();

        var relay = new RelayBuilder()
            .WithId(relayId)
            .WithIsManual(false)
            .WithIsActive(false)
            .Build();

        _scheduleRepo.GetActiveSchedules(Arg.Any<CancellationToken>())
            .Returns(new[] { schedule });
        _relayRepo.GetManyByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { relay });

        Result result = await _handler.Handle(MakeCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        relay.IsActive.Should().BeTrue();

        await _messageScheduler.Received(1).SchedulePublish(
            Arg.Any<DateTime>(),
            Arg.Is<ChangeRelayStateEvent>(e =>
                e.RelayId == relayId && !e.TargetState),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenScheduleMatchesAndRelayAlreadyActive_DoesNotSetStateButSchedulesTurnOff()
    {
        var relayId = ControlTestConstants.RelayId;

        var schedule = new ScheduleBuilder()
            .WithRelayId(relayId)
            .WithCronExpression(MatchingCron)
            .WithDurationMin(15.0)
            .Build();

        var relay = new RelayBuilder()
            .WithId(relayId)
            .WithIsManual(false)
            .WithIsActive(true)
            .Build();

        _scheduleRepo.GetActiveSchedules(Arg.Any<CancellationToken>())
            .Returns(new[] { schedule });
        _relayRepo.GetManyByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { relay });

        Result result = await _handler.Handle(MakeCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        relay.IsActive.Should().BeTrue();

        await _messageScheduler.Received(1).SchedulePublish(
            Arg.Any<DateTime>(),
            Arg.Is<ChangeRelayStateEvent>(e => e.RelayId == relayId && !e.TargetState),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTwoSchedulesSameRelay_LoadsRelayOnceAndPublishesTwice()
    {
        var relayId = ControlTestConstants.RelayId;

        var schedule1 = new ScheduleBuilder()
            .WithId(Guid.NewGuid())
            .WithRelayId(relayId)
            .WithCronExpression(MatchingCron)
            .WithDurationMin(10.0)
            .Build();

        var schedule2 = new ScheduleBuilder()
            .WithId(Guid.NewGuid())
            .WithRelayId(relayId)
            .WithCronExpression(MatchingCron)
            .WithDurationMin(20.0)
            .Build();

        var relay = new RelayBuilder()
            .WithId(relayId)
            .WithIsManual(false)
            .WithIsActive(false)
            .Build();

        _scheduleRepo.GetActiveSchedules(Arg.Any<CancellationToken>())
            .Returns(new[] { schedule1, schedule2 });
        _relayRepo.GetManyByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { relay });

        Result result = await _handler.Handle(MakeCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _relayRepo.Received(1).GetManyByIds(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Count() == 1 && ids.First() == relayId),
            Arg.Any<CancellationToken>());

        await _messageScheduler.Received(2).SchedulePublish(
            Arg.Any<DateTime>(),
            Arg.Any<ChangeRelayStateEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRelayNotFoundInCache_SkipsAndDoesNotPublish()
    {
        var schedule = new ScheduleBuilder()
            .WithCronExpression(MatchingCron)
            .Build();

        _scheduleRepo.GetActiveSchedules(Arg.Any<CancellationToken>())
            .Returns(new[] { schedule });
        _relayRepo.GetManyByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Relay>());

        Result result = await _handler.Handle(MakeCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _messageScheduler.DidNotReceive()
            .SchedulePublish(Arg.Any<DateTime>(), Arg.Any<ChangeRelayStateEvent>(), Arg.Any<CancellationToken>());
    }
}
