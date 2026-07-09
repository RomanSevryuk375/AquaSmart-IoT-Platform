using Device.Application.Interfaces;

namespace Device.Infrastructure.IntegrationTests.BackgroundJobs;

public class CheckOfflineControllersLogicTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly IControllerOfflineCheckerService _checkerService =
        factory.Services.GetRequiredService<IControllerOfflineCheckerService>();

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CheckAndDisable_WhenLastSeenIsOld_SetsIsOnlineToFalse()
    {
        // Arrange

        Controller deadController = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithMacAddress("AA:AA:AA:AA:AA:AA")
            .Build();

        Controller aliveController = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithDeviceTokenHash("aliveController")
            .Build();

        DbContext.Controllers.AddRange(deadController, aliveController);
        await DbContext.SaveChangesAsync();

        await DbContext.Database.ExecuteSqlRawAsync(
            "UPDATE controllers SET last_seen_at = last_seen_at - interval '10 minutes' WHERE id = {0}",
            deadController.Id);

        // Act
        Result result = await _checkerService.CheckAndDisableControllerAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        List<Controller> dbControllers = await DbContext.Controllers.AsNoTracking().ToListAsync();

        Controller dbDead = dbControllers.First(c => c.Id == deadController.Id);
        Controller dbAlive = dbControllers.First(c => c.Id == aliveController.Id);

        dbDead.IsOnline.Should().BeFalse();
        dbAlive.IsOnline.Should().BeTrue();
    }
}
