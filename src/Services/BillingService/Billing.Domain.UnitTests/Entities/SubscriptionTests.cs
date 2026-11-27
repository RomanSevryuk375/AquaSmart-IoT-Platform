namespace Billing.Domain.UnitTests.Entities;

public class SubscriptionTests
{
    [Fact]
    public void CreateActive_WithEmptyId_ReturnsFailure()
    {
        // Arrange
        Plan plan = new PlanBuilder().Build();

        // Act
        Result<Subscription> result = Subscription.CreateActive(
            Guid.Empty,
            BillingTestConstants.CustomerId,
            plan,
            BillingTestConstants.PaymentMethodId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Subscription.Invalid");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreateActive_WithInactivePlan_ReturnsFailure()
    {
        // Arrange
        Plan inactivePlan = new PlanBuilder().WithIsActive(false).Build();

        // Act
        Result<Subscription> result = Subscription.CreateActive(
            BillingTestConstants.SubscriptionId,
            BillingTestConstants.CustomerId,
            inactivePlan,
            BillingTestConstants.PaymentMethodId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Subscription.Invalid");
    }

    [Fact]
    public void CreateActive_PaidPlanWithoutPaymentMethod_ReturnsFailure()
    {
        // Arrange
        Plan paidPlan = new PlanBuilder().WithPrice(19.99m).Build();

        // Act
        Result<Subscription> result = Subscription.CreateActive(
            BillingTestConstants.SubscriptionId,
            BillingTestConstants.CustomerId,
            paidPlan,
            paymentMethodId: null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Subscription.Invalid");
    }

    [Fact]
    public void CreateTrial_WithPlanWithoutTrialDays_ReturnsFailure()
    {
        // Arrange
        Plan noTrialPlan = new PlanBuilder().WithTrialPeriodDays(0).Build();

        // Act
        Result<Subscription> result = Subscription.CreateTrial(
            BillingTestConstants.SubscriptionId,
            BillingTestConstants.CustomerId,
            noTrialPlan);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Subscription.Invalid");
    }

    [Fact]
    public void CreateActive_WithValidData_ReturnsActiveAndRaisesActivatedEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        Plan plan = new PlanBuilder().Build();
        var utcNow = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<Subscription> result = Subscription.CreateActive(
            id,
            customerId,
            plan,
            BillingTestConstants.PaymentMethodId,
            utcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Subscription sub = result.Value;
        sub.Id.Should().Be(id);
        sub.CustomerId.Should().Be(customerId);
        sub.PlanId.Should().Be(plan.Id);
        sub.Status.Should().Be(SubscriptionStatus.Active);
        sub.TrialEnd.Should().BeNull();
        sub.Period.StartDate.Should().Be(utcNow);
        sub.Period.EndDate.Should().Be(utcNow.AddMonths(1));
        sub.Version.Should().NotBeEmpty();

        sub.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SubscriptionActivatedDomainEvent>();

        SubscriptionActivatedDomainEvent evt = sub.DomainEvents.OfType<SubscriptionActivatedDomainEvent>().Single();
        evt.SubscriptionId.Should().Be(sub.Id);
        evt.CustomerId.Should().Be(customerId);
        evt.PlanId.Should().Be(plan.Id);
    }

    [Fact]
    public void RenewPeriod_WhenActive_ExtendsPeriodRaisesRenewedEventAndIncrementsVersion()
    {
        // Arrange
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Subscription sub = new SubscriptionBuilder().WithUtcNow(start).Build();
        Guid initialVersion = sub.Version;

        // Act
        Result result = sub.RenewPeriod(BillingInterval.Month, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(SubscriptionStatus.Active);
        sub.Period.StartDate.Should().Be(start.AddMonths(1));
        sub.Period.EndDate.Should().Be(start.AddMonths(2));
        sub.Version.Should().NotBe(initialVersion);

        sub.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SubscriptionRenewedDomainEvent>();
    }

    [Fact]
    public void RecordFailedPayment_IncrementsAttemptsAndSetsPastDue()
    {
        // Arrange
        Subscription sub = new SubscriptionBuilder().Build();
        Guid initialVersion = sub.Version;
        DateTime retryAt = DateTime.UtcNow.AddDays(3);

        // Act
        Result result = sub.RecordFailedPayment(retryAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(SubscriptionStatus.PastDue);
        sub.FailedPaymentAttempts.Should().Be(1);
        sub.NextRetryAt.Should().Be(retryAt);
        sub.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void CancelAtEndOfPeriod_WhenAlreadyScheduled_IsIdempotentAndDoesNotIncrementVersion()
    {
        // Arrange
        Subscription sub = new SubscriptionBuilder().Build();
        sub.CancelAtEndOfPeriod();
        sub.ClearDomainEvents();
        Guid versionAfterCancel = sub.Version;

        // Act
        Result result = sub.CancelAtEndOfPeriod();

        // Assert
        result.IsSuccess.Should().BeTrue();
        sub.CancelAtPeriodEnd.Should().BeTrue();
        sub.Version.Should().Be(versionAfterCancel);
        sub.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void CancelImmediately_WhenActive_SetsCanceledRaisesEventsAndIncrementsVersion()
    {
        // Arrange
        Subscription sub = new SubscriptionBuilder().Build();
        Guid initialVersion = sub.Version;

        // Act
        Result result = sub.CancelImmediately();

        // Assert
        result.IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(SubscriptionStatus.Canceled);
        sub.CancelAtPeriodEnd.Should().BeFalse();
        sub.CanceledAt.Should().NotBeNull();
        sub.Version.Should().NotBe(initialVersion);
        sub.DomainEvents.Should().HaveCount(2);
        sub.DomainEvents.Should().ContainSingle(e => e is SubscriptionCanceledDomainEvent);
        sub.DomainEvents.Should().ContainSingle(e => e is SubscriptionDowngradedDomainEvent);
    }

    [Fact]
    public void Resume_WhenCancelAtPeriodEnd_ClearsCancellationAndIncrementsVersion()
    {
        // Arrange
        Subscription sub = new SubscriptionBuilder().Build();
        sub.CancelAtEndOfPeriod();
        Guid versionBeforeResume = sub.Version;

        // Act
        Result result = sub.Resume();

        // Assert
        result.IsSuccess.Should().BeTrue();
        sub.CancelAtPeriodEnd.Should().BeFalse();
        sub.CanceledAt.Should().BeNull();
        sub.Version.Should().NotBe(versionBeforeResume);
    }

    [Fact]
    public void ChangePlan_ToSamePlan_DoesNotIncrementVersionOrRaiseEvent()
    {
        // Arrange
        Plan plan = new PlanBuilder().Build();
        Subscription sub = new SubscriptionBuilder().WithPlan(plan).Build();
        Guid initialVersion = sub.Version;

        // Act
        Result result = sub.ChangePlan(plan);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sub.Version.Should().Be(initialVersion);
        sub.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangePlan_ToNewActivePlan_UpdatesPlanPriceRaisesEventAndIncrementsVersion()
    {
        // Arrange
        Plan oldPlan = new PlanBuilder().WithPrice(10m).Build();
        Plan newPlan = new PlanBuilder().WithId(Guid.NewGuid()).WithPrice(29.99m).Build();
        Subscription sub = new SubscriptionBuilder().WithPlan(oldPlan).Build();
        Guid initialVersion = sub.Version;

        // Act
        Result result = sub.ChangePlan(newPlan);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sub.PlanId.Should().Be(newPlan.Id);
        sub.Price.Should().Be(newPlan.Price);
        sub.Version.Should().NotBe(initialVersion);
        sub.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SubscriptionActivatedDomainEvent>();
    }

    [Fact]
    public void MarkUnpaid_WhenActive_SetsUnpaidRaisesDowngradedEvent()
    {
        // Arrange
        Subscription sub = new SubscriptionBuilder().Build();
        Guid initialVersion = sub.Version;

        // Act
        Result result = sub.MarkUnpaid("Payment failed 3 times.");

        // Assert
        result.IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(SubscriptionStatus.Unpaid);
        sub.Version.Should().NotBe(initialVersion);
        sub.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SubscriptionDowngradedDomainEvent>();
    }
}
