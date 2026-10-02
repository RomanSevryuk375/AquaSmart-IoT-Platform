using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Features.RelayCommands.Query.GetPending;

namespace Device.Infrastructure.IntegrationTests.Features.RelayCommands;

public class GetPendingCommandsHandlerTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetPending_WithValidControllerId_ReturnsCommandsAndMarksAsSent()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        Relay relay = new RelayBuilder()
            .WithControllerId(controller.Id)
            .Build();

        RelayCommand cmd1 = new RelayCommandBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        RelayCommand cmd2 = new RelayCommandBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.AddRange(cmd1, cmd2);
        await DbContext.SaveChangesAsync();

        var query = new GetPendingCommandsQuery
        {
            ControllerId = controller.Id
        };

        // Act
        Result<IReadOnlyList<RelayCommandDto>> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        IReadOnlyList<RelayCommandDto> commands = result.Value;

        commands.Should().HaveCount(2);
        commands.Should().AllSatisfy(c =>
        {
            c.Status.Should().Be(CommandStatus.Sent);
            c.AttemptCount.Should().Be(1);
            c.ProcessedAt.Should().NotBeNull();
            c.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        });

        List<RelayCommand> dbCommands = await DbContext.RelayCommands.AsNoTracking().ToListAsync();
        dbCommands.Should().AllSatisfy(c => c.Status.Should().Be(CommandStatus.Sent));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetPending_WhenNoPendingCommands_ReturnsEmptyList()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var query = new GetPendingCommandsQuery { ControllerId = controller.Id };

        // Act
        Result<IReadOnlyList<RelayCommandDto>> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetPending_WithSentButTimedOutCommand_PicksUpCommandForRetry()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        Relay relay = new RelayBuilder()
            .WithControllerId(controller.Id)
            .Build();

        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        command.MarkAsSent();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.Add(command);
        await DbContext.SaveChangesAsync();

        await DbContext.Database.ExecuteSqlRawAsync(
            "UPDATE relay_command_queues SET processed_at = processed_at - interval '2 minutes'");

        var query = new GetPendingCommandsQuery
        {
            ControllerId = controller.Id
        };

        // Act
        Result<IReadOnlyList<RelayCommandDto>> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        RelayCommandDto retriedCommand = result.Value[0];
        retriedCommand.Status.Should().Be(CommandStatus.Sent);
        retriedCommand.AttemptCount.Should().Be(2);
    }
}
