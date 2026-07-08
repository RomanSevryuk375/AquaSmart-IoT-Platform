using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.Reminders.Queries.GetAllReminders;
using Notification.Application.Features.Reminders.Queries.Shared;
using Notification.Domain.Entities;
using Notification.Infrastructure.IntegrationTests.Infrastructure;
using Notification.TestShared.Builders;

namespace Notification.Infrastructure.IntegrationTests.Features.Reminders;

public class GetAllRemindersHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldReturnOnlyCurrentUserReminders_WhenMultipleUsersHaveReminders()
    {
        // Arrange
        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();

        User userA = new UserBuilder()
            .WithId(userAId)
            .WithEmail("userA@example.com")
            .WithPhoneNumber("+375291112233")
            .Build();

        User userB = new UserBuilder()
            .WithId(userBId)
            .WithEmail("userB@example.com")
            .WithPhoneNumber("+375291112234")
            .Build();

        var ecosystemAId = Guid.NewGuid();
        var ecosystemBId = Guid.NewGuid();

        Ecosystem ecosystemA = new EcosystemBuilder()
            .WithId(ecosystemAId)
            .WithUserId(userAId)
            .WithName("Ecosystem A")
            .Build();

        Ecosystem ecosystemB = new EcosystemBuilder()
            .WithId(ecosystemBId)
            .WithUserId(userBId)
            .WithName("Ecosystem B")
            .Build();

        var reminderAId = Guid.NewGuid();
        var reminderBId = Guid.NewGuid();

        Reminder reminderA = new ReminderBuilder()
            .WithId(reminderAId)
            .WithUserId(userAId)
            .WithEcosystemId(ecosystemAId)
            .WithTaskName("User A Task")
            .Build();

        Reminder reminderB = new ReminderBuilder()
            .WithId(reminderBId)
            .WithUserId(userBId)
            .WithEcosystemId(ecosystemBId)
            .WithTaskName("User B Task")
            .Build();

        DbContext.Set<User>().AddRange(userA, userB);
        DbContext.Set<Ecosystem>().AddRange(ecosystemA, ecosystemB);
        DbContext.Set<Reminder>().AddRange(reminderA, reminderB);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        UserContext.UserId = userAId;

        var query = new GetAllRemindersQuery
        {
            UserId = UserContext.UserId,
            Skip = 0,
            Take = 10
        };

        // Act
        Result<IReadOnlyList<ReminderDto>> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().ContainSingle();
        result.Value[0].Id.Should().Be(reminderAId);
        result.Value[0].UserId.Should().Be(userAId);
        result.Value[0].TaskName.Should().Be("User A Task");
    }
}
