using Billing.Domain.Enums;
using Billing.Domain.Events;
using Billing.Domain.ValueObjects;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;

namespace Billing.Domain.Entities;

public sealed class Subscription : AggregateRoot, IEntity
{
    public const int MaxGatewaySubscriptionIdLength = 128;

    private Subscription(
        Guid id,
        Guid customerId,
        Guid planId,
        Guid? paymentMethodId,
        SubscriptionStatus status,
        BillingMoney price,
        DateRange period,
        DateTime? trialEnd,
        DateTime createdAt)
    {
        Id = id;
        CustomerId = customerId;
        PlanId = planId;
        PaymentMethodId = paymentMethodId;
        Status = status;
        Price = price;
        Period = period;
        TrialEnd = trialEnd;
        CreatedAt = createdAt;
        IncrementVersion();
    }

#pragma warning disable CS8618
    private Subscription() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid PlanId { get; private set; }
    public Guid? PaymentMethodId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public BillingMoney Price { get; private set; }
    public DateRange Period { get; private set; }
    public DateTime? TrialEnd { get; private set; }
    public bool CancelAtPeriodEnd { get; private set; }
    public DateTime? CanceledAt { get; private set; }
    public int FailedPaymentAttempts { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public string? GatewaySubscriptionId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Subscription> CreateTrial(
        Guid id,
        Guid customerId,
        Plan plan,
        Guid? paymentMethodId = null,
        DateTime? utcNow = null) =>
        CreateCore(id, customerId, plan, paymentMethodId, isTrial: true, utcNow);

    public static Result<Subscription> CreateActive(
        Guid id,
        Guid customerId,
        Plan plan,
        Guid? paymentMethodId = null,
        DateTime? utcNow = null) =>
        CreateCore(id, customerId, plan, paymentMethodId, isTrial: false, utcNow);

    private static Result<Subscription> CreateCore(
        Guid id,
        Guid customerId,
        Plan plan,
        Guid? paymentMethodId,
        bool isTrial,
        DateTime? utcNow)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "Subscription Id", errors);
        DomainGuard.CheckNotEmpty(customerId, nameof(CustomerId), errors);

        if (plan is null)
        {
            errors.Add("Plan is required.");
            return DomainGuard.ValidationFailure<Subscription>(errors);
        }

        if (!plan.IsActive)
        {
            errors.Add("Cannot subscribe to an inactive plan.");
        }

        if (isTrial && plan.TrialPeriodDays <= 0)
        {
            errors.Add("Selected plan does not support a trial period.");
        }

        if (!isTrial && plan.Price.Amount > 0 && (!paymentMethodId.HasValue ||
                                                   paymentMethodId.Value == Guid.Empty))
        {
            errors.Add("PaymentMethodId is required for a paid active subscription.");
        }

        DateTime now = utcNow ?? DateTime.UtcNow;
        DateTime periodEnd = isTrial
            ? now.AddDays(plan.TrialPeriodDays)
            : plan.CalculateNextPeriodEnd(now);

        DateRange? period = DomainGuard.Collect(DateRange.Create(now, periodEnd), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Subscription>(errors);
        }

        Guid? normalizedMethodId = paymentMethodId is { } methodId && methodId != Guid.Empty
            ? methodId
            : null;

        var subscription = new Subscription(
            id,
            customerId,
            plan.Id,
            normalizedMethodId,
            isTrial ? SubscriptionStatus.Trialing : SubscriptionStatus.Active,
            plan.Price,
            period!,
            isTrial ? periodEnd : null,
            now);

        subscription.RaiseEvent(new SubscriptionActivatedDomainEvent
        {
            CustomerId = customerId,
            SubscriptionId = subscription.Id,
            PlanId = plan.Id,
            PeriodEnd = periodEnd
        });

        return Result<Subscription>.Success(subscription);
    }

    public Result RenewPeriod(BillingInterval interval, int intervalCount)
    {
        if (Status is not (SubscriptionStatus.Active or
                           SubscriptionStatus.Trialing or
                           SubscriptionStatus.PastDue))
        {
            return Result.Failure(Error.Conflict<Subscription>(
                $"Cannot renew subscription in status {Status}."));
        }

        if (CancelAtPeriodEnd)
        {
            return Result.Failure(Error.Conflict<Subscription>(
                "Cannot renew a subscription scheduled for cancellation at period end."));
        }

        Result intervalCheck = DomainGuard.RequireEnum<Subscription, BillingInterval>(
            interval, "billing interval");
        if (intervalCheck.IsFailure)
        {
            return intervalCheck;
        }

        Result countCheck = DomainGuard.RequirePositive<Subscription>(intervalCount, "IntervalCount");
        if (countCheck.IsFailure)
        {
            return countCheck;
        }

        DateTime newStart = Period.EndDate;
        DateTime newEnd = interval == BillingInterval.Month
            ? newStart.AddMonths(intervalCount)
            : newStart.AddYears(intervalCount);

        Result<DateRange> periodResult = DateRange.Create(newStart, newEnd);
        if (periodResult.IsFailure)
        {
            return Result.Failure(periodResult.Error);
        }

        Period = periodResult.Value;
        Status = SubscriptionStatus.Active;
        TrialEnd = null;
        FailedPaymentAttempts = 0;
        NextRetryAt = null;

        IncrementVersion();
        RaiseEvent(new SubscriptionRenewedDomainEvent
        {
            CustomerId = CustomerId,
            SubscriptionId = Id,
            PlanId = PlanId,
            NewPeriodEnd = newEnd
        });

        return Result.Success();
    }

    public Result RecordFailedPayment(DateTime? nextRetryAt)
    {
        if (Status is not (SubscriptionStatus.Active or
                           SubscriptionStatus.Trialing or
                           SubscriptionStatus.PastDue))
        {
            return Result.Failure(Error.Conflict<Subscription>(
                $"Cannot mark subscription as PastDue from status {Status}."));
        }

        FailedPaymentAttempts++;
        NextRetryAt = nextRetryAt;
        Status = SubscriptionStatus.PastDue;

        IncrementVersion();
        return Result.Success();
    }

    public Result MarkUnpaid(string reason = "Payment attempts exhausted.")
    {
        if (Status is SubscriptionStatus.Unpaid or SubscriptionStatus.Expired)
        {
            return Result.Failure(Error.Conflict<Subscription>(
                $"Subscription is already in terminal status {Status}."));
        }

        Status = SubscriptionStatus.Unpaid;
        NextRetryAt = null;

        IncrementVersion();
        RaiseEvent(new SubscriptionDowngradedDomainEvent
        {
            CustomerId = CustomerId,
            SubscriptionId = Id,
            Reason = reason
        });

        return Result.Success();
    }

    public Result MarkExpired()
    {
        if (Status is SubscriptionStatus.Expired)
        {
            return Result.Success();
        }

        Status = SubscriptionStatus.Expired;
        NextRetryAt = null;

        IncrementVersion();
        RaiseEvent(new SubscriptionDowngradedDomainEvent
        {
            CustomerId = CustomerId,
            SubscriptionId = Id,
            Reason = "Subscription period expired after cancellation."
        });

        return Result.Success();
    }

    public Result CancelAtEndOfPeriod(DateTime? utcNow = null)
    {
        if (Status is not (SubscriptionStatus.Trialing or
                           SubscriptionStatus.Active or
                           SubscriptionStatus.PastDue))
        {
            return Result.Failure(Error.Conflict<Subscription>(
                $"Cannot cancel subscription in status {Status}."));
        }

        if (CancelAtPeriodEnd)
        {
            return Result.Success();
        }

        DateTime canceledAt = utcNow ?? DateTime.UtcNow;
        CancelAtPeriodEnd = true;
        CanceledAt = canceledAt;

        IncrementVersion();
        RaiseEvent(new SubscriptionCanceledDomainEvent
        {
            CustomerId = CustomerId,
            SubscriptionId = Id,
            CancelAtPeriodEnd = true,
            CanceledAt = canceledAt
        });

        return Result.Success();
    }

    public Result CancelImmediately(DateTime? utcNow = null)
    {
        if (Status is SubscriptionStatus.Canceled or SubscriptionStatus.Expired)
        {
            return Result.Failure(Error.Conflict<Subscription>(
                $"Subscription is already in status {Status}."));
        }

        DateTime canceledAt = utcNow ?? DateTime.UtcNow;
        Status = SubscriptionStatus.Canceled;
        CancelAtPeriodEnd = false;
        CanceledAt = canceledAt;
        NextRetryAt = null;

        IncrementVersion();
        RaiseEvent(new SubscriptionCanceledDomainEvent
        {
            CustomerId = CustomerId,
            SubscriptionId = Id,
            CancelAtPeriodEnd = false,
            CanceledAt = canceledAt
        });
        RaiseEvent(new SubscriptionDowngradedDomainEvent
        {
            CustomerId = CustomerId,
            SubscriptionId = Id,
            Reason = "Subscription canceled immediately."
        });

        return Result.Success();
    }

    public Result Resume()
    {
        if (!CancelAtPeriodEnd || Status is not (SubscriptionStatus.Trialing or
                                                 SubscriptionStatus.Active or
                                                 SubscriptionStatus.PastDue))
        {
            return Result.Failure(Error.Conflict<Subscription>(
                "Only an active subscription scheduled for cancellation at period end can be resumed."));
        }

        CancelAtPeriodEnd = false;
        CanceledAt = null;

        IncrementVersion();
        return Result.Success();
    }

    public Result ChangePlan(Plan newPlan)
    {
        if (newPlan is null)
        {
            return Result.Failure(Error.Validation<Subscription>("New plan is required."));
        }

        if (!newPlan.IsActive)
        {
            return Result.Failure(Error.Validation<Subscription>("Cannot switch to an inactive plan."));
        }

        if (Status is not (SubscriptionStatus.Active or SubscriptionStatus.Trialing))
        {
            return Result.Failure(Error.Conflict<Subscription>(
                $"Cannot change plan while subscription is in status {Status}."));
        }

        if (PlanId == newPlan.Id)
        {
            return Result.Success();
        }

        PlanId = newPlan.Id;
        Price = newPlan.Price;
        CancelAtPeriodEnd = false;
        CanceledAt = null;

        IncrementVersion();
        RaiseEvent(new SubscriptionActivatedDomainEvent
        {
            CustomerId = CustomerId,
            SubscriptionId = Id,
            PlanId = newPlan.Id,
            PeriodEnd = Period.EndDate
        });

        return Result.Success();
    }

    public Result UpdatePaymentMethod(Guid paymentMethodId)
    {
        Result idCheck = DomainGuard.RequireNotEmpty<Subscription>(paymentMethodId, nameof(PaymentMethodId));
        if (idCheck.IsFailure)
        {
            return idCheck;
        }

        if (PaymentMethodId == paymentMethodId)
        {
            return Result.Success();
        }

        PaymentMethodId = paymentMethodId;
        IncrementVersion();
        return Result.Success();
    }

    public Result AttachGatewaySubscription(string gatewaySubscriptionId)
    {
        Result<string> gatewayIdResult = DomainGuard.RequireText<Subscription>(
            gatewaySubscriptionId, MaxGatewaySubscriptionIdLength, nameof(GatewaySubscriptionId));
        if (gatewayIdResult.IsFailure)
        {
            return Result.Failure(gatewayIdResult.Error);
        }

        if (string.Equals(GatewaySubscriptionId, gatewayIdResult.Value, StringComparison.Ordinal))
        {
            return Result.Success();
        }

        GatewaySubscriptionId = gatewayIdResult.Value;
        IncrementVersion();
        return Result.Success();
    }
}
