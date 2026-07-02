namespace Device.Domain.UnitTests.Entities;

public class RelayCommandTests
{
    [Fact]
    public void Create_ReturnsCommandWithPendingStatus()
    {
        // Arrange
        DateTime expireAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        Result<RelayCommand> result = RelayCommand.Create(
            TestConstants.RelayId,
            TestConstants.ControllerId,
            TestConstants.RelayId,
            targetState: true,
            expireAt);

        // Assert
        result.IsSuccess.Should().BeTrue();

        RelayCommand command = result.Value;
        command.Id.Should().Be(TestConstants.RelayId);
        command.Status.Should().Be(CommandStatus.Pending);
        command.AttemptCount.Should().Be(0);
        command.ExpireAt.Should().Be(expireAt);
        command.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullExpireAt_SetsDefaultExpiration()
    {
        // Act
        Result<RelayCommand> result = RelayCommand.Create(
            TestConstants.RelayId,
            TestConstants.ControllerId,
            TestConstants.RelayId,
            targetState: true,
            expireAt: null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ExpireAt.Should().NotBeNull();
        result.Value.ExpireAt!.Value.Should().BeAfter(DateTime.UtcNow.AddMinutes(4));
    }

    [Fact]
    public void MarkAsSent_WhenPending_UpdatesStatusAndAttemptCount()
    {
        // Arrange
        RelayCommand command = new RelayCommandBuilder().Build();

        // Act
        command.MarkAsSent();

        // Assert
        command.Status.Should().Be(CommandStatus.Sent);
        command.AttemptCount.Should().Be(1);
        command.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsSent_WhenCompleted_DoesNothing()
    {
        // Arrange
        RelayCommand command = new RelayCommandBuilder().Build();
        command.MarkAsCompleted();
        DateTime? processedAtBefore = command.ProcessedAt;

        // Act
        command.MarkAsSent();

        // Assert
        command.Status.Should().Be(CommandStatus.Completed);
        command.AttemptCount.Should().Be(0);
        command.ProcessedAt.Should().Be(processedAtBefore);
    }

    [Fact]
    public void MarkAsFailed_UpdatesStatusAndErrorMessage()
    {
        // Arrange
        RelayCommand command = new RelayCommandBuilder().Build();
        string errorMessage = "Device timeout";

        // Act
        command.MarkAsFailed(errorMessage);

        // Assert
        command.Status.Should().Be(CommandStatus.Failed);
        command.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void MarkAsCompleted_UpdatesStatusAndProcessedAt()
    {
        // Arrange
        RelayCommand command = new RelayCommandBuilder().Build();

        // Act
        command.MarkAsCompleted();

        // Assert
        command.Status.Should().Be(CommandStatus.Completed);
        command.ProcessedAt.Should().NotBeNull();
    }
}
