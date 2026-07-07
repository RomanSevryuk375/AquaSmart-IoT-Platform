using Identity.TestShared.Constants;
using IdentityService.Application.Features.BackgroundJobs.Commands.CleanIncorrectTokens;

namespace Identity.Infrastructure.IntegrationTests.Features.BackgroundJobs.Commands.CleanIncorrectTokens;

public class CleanIncorrectTokensHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldKeepOnlyValidToken_WhenCleanupExecutes()
    {
        // Arrange
        Subscription subscription = new SubscriptionBuilder()
            .WithId(IdentityTestConstants.SubscriptionId)
            .Build();

        User user = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Token Owner")
            .WithEmail("owner@example.com")
            .WithSubscriptionId(subscription.Id)
            .Build();

        DbContext.Subscriptions.Add(subscription);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Seed 4 Refresh Tokens
        RefreshToken validToken = new RefreshTokenBuilder()
            .WithUserId(user.Id)
            .WithTokenHash("hash-valid")
            .Build();

        RefreshToken usedToken = new RefreshTokenBuilder()
            .WithUserId(user.Id)
            .WithTokenHash("hash-used")
            .AsUsed()
            .Build();

        RefreshToken revokedToken = new RefreshTokenBuilder()
            .WithUserId(user.Id)
            .WithTokenHash("hash-revoked")
            .AsRevoked()
            .Build();

        RefreshToken expiredToken = new RefreshTokenBuilder()
            .WithUserId(user.Id)
            .WithTokenHash("hash-expired")
            .Build();

        DbContext.RefreshTokens.AddRange(validToken, usedToken, revokedToken, expiredToken);

        DbContext.Entry(expiredToken).Property(x => x.ExpiredAt).CurrentValue = DateTime.UtcNow.AddDays(-2);

        await DbContext.SaveChangesAsync();

        var command = new CleanIncorrectTokensCommand();

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        List<RefreshToken> remainingTokens = await DbContext.RefreshTokens
            .AsNoTracking()
            .ToListAsync();

        remainingTokens.Should().ContainSingle();
        RefreshToken remainingToken = remainingTokens.Single();
        remainingToken.TokenHash.Should().Be("hash-valid");
        remainingToken.IsUsed.Should().BeFalse();
        remainingToken.IsRevoked.Should().BeFalse();
        remainingToken.ExpiredAt.Should().BeAfter(DateTime.UtcNow);
    }
}
