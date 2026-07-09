using Contracts.Results;
using FluentAssertions;
using Notification.Domain.Entities;
using Notification.TestShared.Builders;

namespace Notification.Domain.UnitTests.Entities;

public class EcosystemTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndInitializesProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string name = "My Tropical Reef";

        // Act
        Result<Ecosystem> result = Ecosystem.Create(id, userId, name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.UserId.Should().Be(userId);
        result.Value.EcosystemName.Value.Should().Be(name);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.Version.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_WithInvalidName_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string invalidName = "";

        // Act
        Result<Ecosystem> result = Ecosystem.Create(id, userId, invalidName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
    }

    [Fact]
    public void SetName_WithValidName_UpdatesNameAndIncrementsVersion()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Guid initialVersion = ecosystem.Version;
        string newName = "Updated Reef Tank";

        // Act
        Result result = ecosystem.SetName(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ecosystem.EcosystemName.Value.Should().Be(newName);
        ecosystem.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetName_WithInvalidName_ReturnsFailureAndDoesNotIncrementVersion()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder().Build();
        Guid initialVersion = ecosystem.Version;
        string invalidName = "   ";

        // Act
        Result result = ecosystem.SetName(invalidName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        ecosystem.Version.Should().Be(initialVersion);
    }
}
