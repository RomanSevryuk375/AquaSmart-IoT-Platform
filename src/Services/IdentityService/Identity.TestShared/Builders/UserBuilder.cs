using System;
using Contracts.Results;
using Identity.TestShared.Constants;
using IdentityService.Domain.Entities;

namespace Identity.TestShared.Builders;

public class UserBuilder
{
    private Guid _id = IdentityTestConstants.UserId;
    private string _name = "Test User";
    private string _email = "test.user@example.com";
    private string _phoneNumber = "+375291112233";
    private Guid _subscriptionId = IdentityTestConstants.SubscriptionId;
    private string _timeZone = "Europe/Minsk";

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }

    public UserBuilder WithSubscriptionId(Guid subscriptionId)
    {
        _subscriptionId = subscriptionId;
        return this;
    }

    public UserBuilder WithTimeZone(string timeZone)
    {
        _timeZone = timeZone;
        return this;
    }

    public User Build()
    {
        Result<User> result = User.Create(
            _id,
            _name,
            _email,
            _phoneNumber,
            _subscriptionId,
            _timeZone);

        if (result.IsFailure)
        {
            throw new ArgumentException($"UserBuilder failed: {result.Error.Message}");
        }

        User user = result.Value;
        user.ClearDomainEvents();
        return user;
    }
}
