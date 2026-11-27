using Billing.Domain.Entities;
using Billing.TestShared.Constants;
using BuildingBlocks.Domain.Results;

namespace Billing.TestShared.Builders;

public sealed class SubscriptionBuilder
{
    private Guid _id = BillingTestConstants.SubscriptionId;
    private Guid _customerId = BillingTestConstants.CustomerId;
    private Plan? _plan;
    private Guid? _paymentMethodId = BillingTestConstants.PaymentMethodId;
    private bool _isTrial;
    private DateTime? _utcNow;

    public SubscriptionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SubscriptionBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public SubscriptionBuilder WithPlan(Plan plan)
    {
        _plan = plan;
        return this;
    }

    public SubscriptionBuilder WithPaymentMethodId(Guid? paymentMethodId)
    {
        _paymentMethodId = paymentMethodId;
        return this;
    }

    public SubscriptionBuilder AsTrial(int trialDays = 14)
    {
        _isTrial = true;
        _plan ??= new PlanBuilder().WithTrialPeriodDays(trialDays).Build();
        return this;
    }

    public SubscriptionBuilder WithUtcNow(DateTime utcNow)
    {
        _utcNow = utcNow;
        return this;
    }

    public Subscription Build()
    {
        Plan plan = _plan ?? new PlanBuilder().Build();

        Result<Subscription> result = _isTrial
            ? Subscription.CreateTrial(_id, _customerId, plan, _paymentMethodId, _utcNow)
            : Subscription.CreateActive(_id, _customerId, plan, _paymentMethodId, _utcNow);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to build Subscription: {result.Error.Message}");
        }

        Subscription subscription = result.Value;
        subscription.ClearDomainEvents();
        return subscription;
    }
}
