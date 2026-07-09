using System;
using System.Collections.Generic;
using Contracts.Results;
using Identity.TestShared.Constants;
using IdentityService.Domain.Entities;

namespace Identity.TestShared.Builders;

public class SubscriptionBuilder
{
    private Guid _id = IdentityTestConstants.SubscriptionId;
    private string _name = "Premium Plan";
    private decimal _price = 19.99m;
    private int _durationDays = 30;
    private List<string> _permissions = new() { "read:telemetry", "write:commands" };

    public SubscriptionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SubscriptionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SubscriptionBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public SubscriptionBuilder WithDurationDays(int durationDays)
    {
        _durationDays = durationDays;
        return this;
    }

    public SubscriptionBuilder WithPermissions(List<string> permissions)
    {
        _permissions = permissions;
        return this;
    }

    public Subscription Build()
    {
        Result<Subscription> result = Subscription.Create(
            _id,
            _name,
            _price,
            _durationDays,
            _permissions);

        if (result.IsFailure)
        {
            throw new ArgumentException($"SubscriptionBuilder failed: {result.Error.Message}");
        }

        Subscription subscription = result.Value;
        subscription.ClearDomainEvents();
        return subscription;
    }
}
