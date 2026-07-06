using Control.Application.Features.Schedules.Commands.ProcessSchedules;

namespace Control.Infrastructure.IntegrationTests.Features.Schedules;

public class ProcessSchedulesHandlerTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldMutateRelayWhenScheduleMatchesCurrentTime()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithIsActive(false)
            .WithIsManual(false)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .WithCronExpression("* * * * *")
            .WithIsEnabled(true)
            .Build();

        DbContext.Set<Ecosystem>().Add(ecosystem);
        DbContext.Set<Relay>().Add(relay);
        DbContext.Set<Schedule>().Add(schedule);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new ProcessSchedulesCommand();

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Relay? updatedRelay = await DbContext.Set<Relay>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == relay.Id);

        updatedRelay.Should().NotBeNull();
        updatedRelay!.IsActive.Should().BeTrue();
    }
}
