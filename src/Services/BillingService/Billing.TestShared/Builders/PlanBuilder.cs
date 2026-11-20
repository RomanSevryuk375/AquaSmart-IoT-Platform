using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Billing.TestShared.Constants;
using BuildingBlocks.Domain.Results;

namespace Billing.TestShared.Builders;

public sealed class PlanBuilder
{
    private Guid _id = BillingTestConstants.PlanId;
    private Guid _subscriptionTypeId = BillingTestConstants.SubscriptionTypeId;
    private string _code = BillingTestConstants.PlanCode;
    private string _name = BillingTestConstants.PlanName;
    private decimal _price = BillingTestConstants.PlanPrice;
    private string _currency = BillingTestConstants.Currency;
    private BillingInterval _billingInterval = BillingInterval.Month;
    private int _intervalCount = 1;
    private int _trialPeriodDays;
    private bool _isActive = true;

    public PlanBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PlanBuilder WithSubscriptionTypeId(Guid subscriptionTypeId)
    {
        _subscriptionTypeId = subscriptionTypeId;
        return this;
    }

    public PlanBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public PlanBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PlanBuilder WithPrice(decimal price, string currency = BillingTestConstants.Currency)
    {
        _price = price;
        _currency = currency;
        return this;
    }

    public PlanBuilder WithBillingInterval(BillingInterval interval, int intervalCount = 1)
    {
        _billingInterval = interval;
        _intervalCount = intervalCount;
        return this;
    }

    public PlanBuilder WithTrialPeriodDays(int trialPeriodDays)
    {
        _trialPeriodDays = trialPeriodDays;
        return this;
    }

    public PlanBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public Plan Build()
    {
        Result<Plan> result = Plan.Create(
            _id,
            _subscriptionTypeId,
            _code,
            _name,
            _price,
            _currency,
            _billingInterval,
            _intervalCount,
            _trialPeriodDays);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to build Plan: {result.Error.Message}");
        }

        Plan plan = result.Value;
        if (!_isActive)
        {
            plan.Deactivate();
        }

        return plan;
    }
}
