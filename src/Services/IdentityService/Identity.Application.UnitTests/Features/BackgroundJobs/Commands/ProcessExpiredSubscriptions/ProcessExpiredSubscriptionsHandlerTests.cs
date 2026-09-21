using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using IdentityService.Application.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;
using IdentityService.Domain.Events;

namespace Identity.Application.UnitTests.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;

public class ProcessExpiredSubscriptionsHandlerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly ProcessExpiredSubscriptionsHandler _handler;

    public ProcessExpiredSubscriptionsHandlerTests()
    {
        _handler = new ProcessExpiredSubscriptionsHandler(_userRepositoryMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoUsersWithExpiredSubscription_ReturnsSuccess()
    {
        // Arrange
        _userRepositoryMock.GetWithExpiredSubscriptionAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new ProcessExpiredSubscriptionsCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUsersHaveExpiredSubscription_DowngradesToFreeAndRaisesDomainEvents()
    {
        // Arrange
        var proId = Guid.Parse(SubscriptionType.Professional);
        var freeId = Guid.Parse(SubscriptionType.Free);

        User user1 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("user1@example.com")
            .WithSubscriptionId(proId)
            .Build();
        user1.SetSubscription(proId, -1);
        user1.ClearDomainEvents();

        User user2 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("user2@example.com")
            .WithSubscriptionId(proId)
            .Build();
        user2.SetSubscription(proId, -5);
        user2.ClearDomainEvents();

        _userRepositoryMock.GetWithExpiredSubscriptionAsync(Arg.Any<CancellationToken>())
            .Returns([user1, user2]);

        var command = new ProcessExpiredSubscriptionsCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        user1.SubscriptionId.Should().Be(freeId);
        user2.SubscriptionId.Should().Be(freeId);

        user1.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SubscriptionDowngradedDomainEvent>()
            .Which.NewSubscriptionId.Should().Be(freeId);

        user2.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SubscriptionDowngradedDomainEvent>()
            .Which.NewSubscriptionId.Should().Be(freeId);
    }
}
