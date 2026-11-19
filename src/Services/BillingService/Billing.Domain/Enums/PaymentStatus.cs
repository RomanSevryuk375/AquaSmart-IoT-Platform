namespace Billing.Domain.Enums;

public enum PaymentStatus
{
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Canceled = 4,
    Refunded = 5
}
