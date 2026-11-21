using Billing.Domain.Entities;
using Billing.TestShared.Constants;
using BuildingBlocks.Domain.Results;

namespace Billing.TestShared.Builders;

public sealed class BillingCustomerBuilder
{
    private Guid _id = BillingTestConstants.CustomerId;
    private Guid _userId = BillingTestConstants.UserId;
    private string _email = BillingTestConstants.CustomerEmail;

    public BillingCustomerBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public BillingCustomerBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public BillingCustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public BillingCustomer Build()
    {
        Result<BillingCustomer> result = BillingCustomer.Create(_id, _userId, _email);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to build BillingCustomer: {result.Error.Message}");
        }

        return result.Value;
    }
}
