// Ignore Spelling: Telemetry

namespace Telemetry.Domain.UnitTests.Entities;

public class EcosystemTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccessAndInitializesProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        Result<Ecosystem> result = Ecosystem.Create(id, controllerId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.ControllerId.Should().Be(controllerId);
        result.Value.UserId.Should().Be(userId);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.Version.Should().BeEmpty();
    }

    [Fact]
    public void Builder_BuildsWithDefaultValues_ReturnsValidEcosystem()
    {
        // Arrange
        var builder = new EcosystemBuilder();

        // Act
        Ecosystem ecosystem = builder.Build();

        // Assert
        ecosystem.Should().NotBeNull();
        ecosystem.Id.Should().NotBeEmpty();
        ecosystem.ControllerId.Should().NotBeEmpty();
        ecosystem.UserId.Should().NotBeEmpty();
        ecosystem.Version.Should().BeEmpty();
    }
}
