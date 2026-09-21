using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.IntegrationEvents.Events.Users;
using IdentityService.Application.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;
using MassTransit;

namespace Identity.Application.UnitTests.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;

public class ProcessExpiredSubscriptionsHandlerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();
    private readonly ProcessExpiredSubscriptionsHandler _handler;

    public ProcessExpiredSubscriptionsHandlerTests()
    {
        _handler = new ProcessExpiredSubscriptionsHandler(_userRepositoryMock, _publishEndpointMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoUsersWithExpiredSubscription_ReturnsSuccessAndPublishesNothing()
    {
        // Arrange
        _userRepositoryMock.GetWithExpiredSubscriptionAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new ProcessExpiredSubscriptionsCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<SubscriptionDowngradedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUsersHaveExpiredSubscription_DowngradesToFreeAndPublishesEvents()
    {
        // Arrange
        Guid proId = Guid.Parse(SubscriptionType.Professional);
        Guid freeId = Guid.Parse(SubscriptionType.Free);

        User user1 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("user1@example.com")
            .WithSubscriptionId(proId)
            .Build();
        user1.SetSubscription(proId, -1);

        User user2 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("user2@example.com")
            .WithSubscriptionId(proId)
            .Build();
        user2.SetSubscription(proId, -5);

        _userRepositoryMock.GetWithExpiredSubscriptionAsync(Arg.Any<CancellationToken>())
            .Returns([user1, user2]);

        var command = new ProcessExpiredSubscriptionsCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        user1.SubscriptionId.Should().Be(freeId);
        user2.SubscriptionId.Should().Be(freeId);

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SubscriptionDowngradedEvent>(e => e.UserId == user1.Id && e.NewSubscriptionId == freeId),
            Arg.Any<CancellationToken>());

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SubscriptionDowngradedEvent>(e => e.UserId == user2.Id && e.NewSubscriptionId == freeId),
            Arg.Any<CancellationToken>());
    }
}
