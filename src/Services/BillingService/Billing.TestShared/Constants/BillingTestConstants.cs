namespace Billing.TestShared.Constants;

public static class BillingTestConstants
{
    public static readonly Guid CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid PlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid SubscriptionTypeId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid SubscriptionId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid PaymentMethodId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid InvoiceId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid InvoiceItemId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid PaymentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public const string CustomerEmail = "billing.customer@example.com";
    public const string PlanCode = "pro-monthly";
    public const string PlanName = "Pro Monthly Plan";
    public const decimal PlanPrice = 19.99m;
    public const string Currency = "USD";
    public const string InvoiceNumberValue = "INV-2026-00001";
    public const string GatewayToken = "pm_tok_test_12345";
    public const string GatewayTransactionId = "txn_test_98765";
}
