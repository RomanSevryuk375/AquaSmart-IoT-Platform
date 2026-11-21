namespace Billing.Domain.Enums;

public enum SubscriptionStatus
{
    Trialing = 1,
    Active = 2,
    PastDue = 3,
    Canceled = 4,
    Unpaid = 5,
    Expired = 6
}
