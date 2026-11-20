namespace Billing.Domain.UnitTests.Entities;

public class PlanTests
{
    [Fact]
    public void Create_WithEmptyId_ReturnsFailure()
    {
        // Act
        Result<Plan> result = Plan.Create(
            Guid.Empty,
            BillingTestConstants.SubscriptionTypeId,
            BillingTestConstants.PlanCode,
            BillingTestConstants.PlanName,
            10m,
            "USD",
            BillingInterval.Month);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Plan.Invalid");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Create_WithNonPositiveIntervalCount_ReturnsFailure(int intervalCount)
    {
        // Act
        Result<Plan> result = Plan.Create(
            BillingTestConstants.PlanId,
            BillingTestConstants.SubscriptionTypeId,
            BillingTestConstants.PlanCode,
            BillingTestConstants.PlanName,
            10m,
            "USD",
            BillingInterval.Month,
            intervalCount: intervalCount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Plan.Invalid");
    }

    [Fact]
    public void Create_WithNegativeTrialPeriodDays_ReturnsFailure()
    {
        // Act
        Result<Plan> result = Plan.Create(
            BillingTestConstants.PlanId,
            BillingTestConstants.SubscriptionTypeId,
            BillingTestConstants.PlanCode,
            BillingTestConstants.PlanName,
            10m,
            "USD",
            BillingInterval.Month,
            trialPeriodDays: -1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Plan.Invalid");
    }

    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndNormalizesFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var subTypeId = Guid.NewGuid();
        var tolerance = TimeSpan.FromSeconds(5);

        // Act
        Result<Plan> result = Plan.Create(
            id,
            subTypeId,
            "  PRO-YEARLY  ",
            "  Pro Yearly  ",
            199.99m,
            "usd",
            BillingInterval.Year,
            intervalCount: 1,
            trialPeriodDays: 14);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Plan plan = result.Value;
        plan.Id.Should().Be(id);
        plan.SubscriptionTypeId.Should().Be(subTypeId);
        plan.Code.Should().Be("pro-yearly");
        plan.Name.Should().Be("Pro Yearly");
        plan.Price.Amount.Should().Be(199.99m);
        plan.Price.Currency.Should().Be("USD");
        plan.BillingInterval.Should().Be(BillingInterval.Year);
        plan.IntervalCount.Should().Be(1);
        plan.TrialPeriodDays.Should().Be(14);
        plan.IsActive.Should().BeTrue();
        plan.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, tolerance);
        plan.Version.Should().NotBeEmpty();
    }

    [Fact]
    public void Deactivate_WhenActive_SetsInactiveAndIncrementsVersion()
    {
        // Arrange
        Plan plan = new PlanBuilder().WithIsActive(true).Build();
        Guid initialVersion = plan.Version;

        // Act
        Result result = plan.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.IsActive.Should().BeFalse();
        plan.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_DoesNotIncrementVersion()
    {
        // Arrange
        Plan plan = new PlanBuilder().WithIsActive(false).Build();
        Guid initialVersion = plan.Version;

        // Act
        Result result = plan.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.IsActive.Should().BeFalse();
        plan.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void Activate_WhenInactive_SetsActiveAndIncrementsVersion()
    {
        // Arrange
        Plan plan = new PlanBuilder().WithIsActive(false).Build();
        Guid initialVersion = plan.Version;

        // Act
        Result result = plan.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.IsActive.Should().BeTrue();
        plan.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void UpdateName_WithInvalidName_ReturnsFailureAndDoesNotMutate()
    {
        // Arrange
        Plan plan = new PlanBuilder().WithName("Initial Plan").Build();
        Guid initialVersion = plan.Version;

        // Act
        Result result = plan.UpdateName("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Plan.Invalid");
        plan.Name.Should().Be("Initial Plan");
        plan.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void CalculateNextPeriodEnd_WithMonthAndYearIntervals_ReturnsExpectedDate()
    {
        // Arrange
        var start = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        Plan monthlyPlan = new PlanBuilder().WithBillingInterval(BillingInterval.Month, 2).Build();
        Plan yearlyPlan = new PlanBuilder().WithBillingInterval(BillingInterval.Year, 1).Build();

        // Assert
        monthlyPlan.CalculateNextPeriodEnd(start).Should().Be(new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc));
        yearlyPlan.CalculateNextPeriodEnd(start).Should().Be(new DateTime(2027, 1, 15, 0, 0, 0, DateTimeKind.Utc));
    }
}
