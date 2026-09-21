using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Features.RelayCommands.Command.DeleteCompleted;

namespace Device.Infrastructure.IntegrationTests.BackgroundJobs;

public class DeleteCompletedCommandsLogicTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteCompleted_RemovesCompletedAndOldExpired_KeepsPendingAndRecentlyExpired()
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

        RelayCommand recentlyExpiredCommand = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .WithExpireAt(DateTime.UtcNow.AddMinutes(-5))
            .Build();

        RelayCommand oldExpiredCommand = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .WithExpireAt(DateTime.UtcNow.AddDays(-2))
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.AddRange(pendingCommand, completedCommand, recentlyExpiredCommand, oldExpiredCommand);
        await DbContext.SaveChangesAsync();

        Result result = await Sender.Send(new DeleteCompletedCommand());

        // Assert
        result.IsSuccess.Should().BeTrue();

        List<RelayCommand> remainingCommands = await DbContext.RelayCommands.AsNoTracking().ToListAsync();

        remainingCommands.Should().HaveCount(2);
        remainingCommands.Select(c => c.Id).Should().Contain([pendingCommand.Id, recentlyExpiredCommand.Id]);
        remainingCommands.Select(c => c.Id).Should().NotContain([completedCommand.Id, oldExpiredCommand.Id]);

        RelayCommand savedPending = remainingCommands.Single(c => c.Id == pendingCommand.Id);
        savedPending.Status.Should().Be(CommandStatus.Pending);

        RelayCommand savedRecentlyExpired = remainingCommands.Single(c => c.Id == recentlyExpiredCommand.Id);
        savedRecentlyExpired.Status.Should().Be(CommandStatus.Pending);
    }
}
