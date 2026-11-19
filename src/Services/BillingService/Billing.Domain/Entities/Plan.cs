using Billing.Domain.Enums;
using Billing.Domain.ValueObjects;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;

namespace Billing.Domain.Entities;

public sealed class Plan : AggregateRoot, IEntity
{
    public const int MaxCodeLength = 64;
    public const int MaxNameLength = 128;

    private Plan(
        Guid id,
        Guid subscriptionTypeId,
        string code,
        string name,
        BillingMoney price,
        BillingInterval billingInterval,
        int intervalCount,
        int trialPeriodDays,
        bool isActive,
        DateTime createdAt)
    {
        Id = id;
        SubscriptionTypeId = subscriptionTypeId;
        Code = code;
        Name = name;
        Price = price;
        BillingInterval = billingInterval;
        IntervalCount = intervalCount;
        TrialPeriodDays = trialPeriodDays;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private Plan() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid SubscriptionTypeId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public BillingMoney Price { get; private set; }
    public BillingInterval BillingInterval { get; private set; }
    public int IntervalCount { get; private set; }
    public int TrialPeriodDays { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Plan> Create(
        Guid id,
        Guid subscriptionTypeId,
        string code,
        string name,
        decimal price,
        string currency,
        BillingInterval billingInterval,
        int intervalCount = 1,
        int trialPeriodDays = 0)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "Plan Id", errors);
        DomainGuard.CheckNotEmpty(subscriptionTypeId, nameof(SubscriptionTypeId), errors);

        string normalizedCode = DomainGuard.CheckText(code, MaxCodeLength, "Plan code", errors).ToLowerInvariant();
        string trimmedName = DomainGuard.CheckText(name, MaxNameLength, "Plan name", errors);
        BillingMoney? priceVo = DomainGuard.Collect(BillingMoney.Create(price, currency), errors);
        DomainGuard.CheckEnum(billingInterval, "billing interval", errors);
        DomainGuard.CheckPositive(intervalCount, nameof(IntervalCount), errors);
        DomainGuard.CheckNonNegative(trialPeriodDays, nameof(TrialPeriodDays), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Plan>(errors);
        }

        return Result<Plan>.Success(new Plan(
            id, subscriptionTypeId,
            normalizedCode, trimmedName,
            priceVo!,
            billingInterval, intervalCount, trialPeriodDays,
            isActive: true, createdAt: DateTime.UtcNow));
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Success();
        }

        IsActive = false;
        IncrementVersion();
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
        {
            return Result.Success();
        }

        IsActive = true;
        IncrementVersion();
        return Result.Success();
    }

    public Result UpdateName(string name)
    {
        Result<string> nameResult = DomainGuard.RequireText<Plan>(name, MaxNameLength, "Plan name");
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        if (string.Equals(Name, nameResult.Value, StringComparison.Ordinal))
        {
            return Result.Success();
        }

        Name = nameResult.Value;
        IncrementVersion();
        return Result.Success();
    }

    public DateTime CalculateNextPeriodEnd(DateTime periodStart) =>
        BillingInterval switch
        {
            BillingInterval.Month => periodStart.AddMonths(IntervalCount),
            BillingInterval.Year => periodStart.AddYears(IntervalCount),
            _ => throw new InvalidOperationException($"Unsupported billing interval: {BillingInterval}")
        };
}
