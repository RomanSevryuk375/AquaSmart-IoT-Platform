using Control.Application.Features.VacationModes.Queries.GetAllVacationModes;
using Control.Application.Features.VacationModes.Queries.Shared;

namespace Control.Infrastructure.IntegrationTests.Features.VacationModes;

public class GetAllVacationModesHandlerTests(
    IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldReturnOnlyUserAVacationModes()
    {
        // Arrange
        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();

        Ecosystem ecosystemA = new EcosystemBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(Guid.NewGuid())
            .WithUserId(userAId)
            .Build();

        Ecosystem ecosystemB = new EcosystemBuilder()
            .WithId(Guid.NewGuid())
            .WithControllerId(Guid.NewGuid())
            .WithUserId(userBId)
            .Build();

        VacationMode vacationA = new VacationModeBuilder()
            .WithId(Guid.NewGuid())
            .WithEcosystemId(ecosystemA.Id)
            .Build();

        VacationMode vacationB = new VacationModeBuilder()
            .WithId(Guid.NewGuid())
            .WithEcosystemId(ecosystemB.Id)
            .Build();

        DbContext.Set<Ecosystem>().Add(ecosystemA);
        DbContext.Set<Ecosystem>().Add(ecosystemB);
        DbContext.Set<VacationMode>().Add(vacationA);
        DbContext.Set<VacationMode>().Add(vacationB);
        await DbContext.SaveChangesAsync();

        UserContext.UserId = userAId;

        var query = new GetAllVacationModesQuery
        {
            EcosystemId = ecosystemA.Id,
            UserId = userAId,
            Take = 10,
            Skip = 0
        };

        // Act
        Result<IReadOnlyList<VacationModeDto>> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(vacationA.Id);
    }
}
