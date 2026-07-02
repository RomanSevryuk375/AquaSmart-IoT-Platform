using Device.Application.Features.RelayCommands.Command.DeleteCompleted;

namespace Device.Infrastructure.IntegrationTests.BackgroundJobs;

public class DeleteCompletedCommandsLogicTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteCompleted_RemovesCompletedAndExpired_KeepsPending()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        Relay relay = new RelayBuilder()
            .WithControllerId(controller.Id)
            .Build();

        RelayCommand pendingCommand = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .WithExpireAt(DateTime.UtcNow.AddMinutes(10))
            .Build();

        RelayCommand completedCommand = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        completedCommand.MarkAsCompleted();

        RelayCommand expiredCommand = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .WithExpireAt(DateTime.UtcNow.AddMinutes(-5))
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.AddRange(pendingCommand, completedCommand, expiredCommand);
        await DbContext.SaveChangesAsync();

        Result result = await Sender.Send(new DeleteCompletedCommand());

        // Assert
        result.IsSuccess.Should().BeTrue();

        List<RelayCommand> remainingCommands = await DbContext.RelayCommands.AsNoTracking().ToListAsync();

        remainingCommands.Should().ContainSingle();
        remainingCommands[0].Id.Should().Be(pendingCommand.Id);
        remainingCommands[0].Status.Should().Be(CommandStatus.Pending);
    }
}
