using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Billing.Domain.ValueObjects;
using Billing.Infrastructure.Persistence;
using Billing.Infrastructure.Persistence.Entities;
using Billing.TestShared.Builders;
using BuildingBlocks.Domain.ValueObjects;
using BuildingBlocks.Infrastructure.Data.Outbox;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Billing.Infrastructure.IntegrationTests.Persistence;

public sealed class BillingDbContextModelTests
{
    private static BillingDbContext CreateDbContext()
    {
        DbContextOptions<BillingDbContext> options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseNpgsql("Host=localhost;Database=billing_db;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new BillingDbContext(options);
    }

    [Fact]
    public void OnModelCreating_ShouldBuildModelAndGeneratePostgresDdl_WithoutErrors()
    {
        // Arrange
        using BillingDbContext context = CreateDbContext();

        // Act
        IModel model = context.Model;
        string ddl = context.Database.GenerateCreateScript();

        // Assert
        model.FindEntityType(typeof(BillingCustomer)).Should().NotBeNull();
        model.FindEntityType(typeof(PaymentMethod)).Should().NotBeNull();
        model.FindEntityType(typeof(Plan)).Should().NotBeNull();
        model.FindEntityType(typeof(Subscription)).Should().NotBeNull();
        model.FindEntityType(typeof(Invoice)).Should().NotBeNull();
        model.FindEntityType(typeof(InvoiceItem)).Should().NotBeNull();
        model.FindEntityType(typeof(Payment)).Should().NotBeNull();
        model.FindEntityType(typeof(IdempotentConsumerEvent)).Should().NotBeNull();
        model.FindEntityType(typeof(OutboxMessage)).Should().NotBeNull();

        ddl.Should().Contain("CREATE SEQUENCE invoice_number_seq");
        ddl.Should().Contain("CREATE TABLE billing_customers");
        ddl.Should().Contain("CREATE TABLE payment_methods");
        ddl.Should().Contain("CREATE TABLE plans");
        ddl.Should().Contain("CREATE TABLE subscriptions");
        ddl.Should().Contain("CREATE TABLE invoices");
        ddl.Should().Contain("CREATE TABLE invoice_items");
        ddl.Should().Contain("CREATE TABLE payments");
        ddl.Should().Contain("CREATE TABLE idempotent_consumer_events");
        ddl.Should().Contain("CREATE TABLE outbox_messages");

        ddl.Should().Contain("current_period_start timestamp with time zone NOT NULL");
        ddl.Should().Contain("current_period_end timestamp with time zone NOT NULL");
        ddl.Should().Contain("period_start timestamp with time zone");
        ddl.Should().Contain("period_end timestamp with time zone");
        ddl.Should().Contain("card_last4 character varying(4)");
        ddl.Should().Contain("card_brand character varying(32)");
        ddl.Should().Contain("price numeric(12,2) NOT NULL");
        ddl.Should().Contain("currency character varying(3) NOT NULL");
        ddl.Should().Contain("amount numeric(12,2) NOT NULL");
        ddl.Should().Contain("subtotal numeric(12,2) NOT NULL");
        ddl.Should().Contain("refunded_amount numeric(12,2) NOT NULL");

        ddl.Should().NotContain("price_amount");
        ddl.Should().NotContain("period_start_date");
        ddl.Should().NotContain("card_details_");
    }

    [Fact]
    public void Model_ShouldConfigureConcurrencyTokens_ForAllVersionedEntities()
    {
        // Arrange
        using BillingDbContext context = CreateDbContext();
        Type[] versionedEntities =
        [
            typeof(BillingCustomer),
            typeof(PaymentMethod),
            typeof(Plan),
            typeof(Subscription),
            typeof(Invoice),
            typeof(Payment)
        ];

        // Act & Assert
        foreach (Type entityType in versionedEntities)
        {
            IEntityType? efType = context.Model.FindEntityType(entityType);
            efType.Should().NotBeNull();
            IProperty? versionProperty = efType!.FindProperty("Version");
            versionProperty.Should().NotBeNull();
            versionProperty!.IsConcurrencyToken.Should().BeTrue($"{entityType.Name} must use optimistic concurrency.");
        }
    }

    [Fact]
    public void Model_ShouldConfigureValueConverters_ForScalarValueObjects()
    {
        // Arrange
        using BillingDbContext context = CreateDbContext();

        // Act
        IProperty? emailProperty = context.Model.FindEntityType(typeof(BillingCustomer))?.FindProperty(nameof(BillingCustomer.Email));
        IProperty? invoiceNumberProperty = context.Model.FindEntityType(typeof(Invoice))?.FindProperty(nameof(Invoice.Number));
        IProperty? subtotalProperty = context.Model.FindEntityType(typeof(Invoice))?.FindProperty(nameof(Invoice.Subtotal));

        // Assert
        emailProperty!.GetValueConverter().Should().NotBeNull();
        emailProperty.GetValueConverter()!.ModelClrType.Should().Be(typeof(EmailAddress));
        emailProperty.GetValueConverter()!.ProviderClrType.Should().Be(typeof(string));

        invoiceNumberProperty!.GetValueConverter().Should().NotBeNull();
        invoiceNumberProperty.GetValueConverter()!.ModelClrType.Should().Be(typeof(InvoiceNumber));
        invoiceNumberProperty.GetValueConverter()!.ProviderClrType.Should().Be(typeof(string));

        subtotalProperty!.GetValueConverter().Should().NotBeNull();
        subtotalProperty.GetValueConverter()!.ModelClrType.Should().Be(typeof(Money));
        subtotalProperty.GetValueConverter()!.ProviderClrType.Should().Be(typeof(decimal));
    }

    [Fact]
    public void ValueConverters_ShouldRoundTrip_ValidDatabaseValues()
    {
        // Arrange
        using BillingDbContext context = CreateDbContext();
        ValueConverter emailConverter = context.Model
            .FindEntityType(typeof(BillingCustomer))!
            .FindProperty(nameof(BillingCustomer.Email))!
            .GetValueConverter()!;
        ValueConverter invoiceNumberConverter = context.Model
            .FindEntityType(typeof(Invoice))!
            .FindProperty(nameof(Invoice.Number))!
            .GetValueConverter()!;
        ValueConverter moneyConverter = context.Model
            .FindEntityType(typeof(Invoice))!
            .FindProperty(nameof(Invoice.Subtotal))!
            .GetValueConverter()!;

        EmailAddress email = EmailAddress.Create("User@AquaSmart.io").Value;
        InvoiceNumber invoiceNumber = InvoiceNumber.Generate(2026, 42).Value;
        Money money = Money.Create(19.95m).Value;

        // Act
        object? rawEmail = emailConverter.ConvertToProvider(email);
        object? parsedEmail = emailConverter.ConvertFromProvider(rawEmail);

        object? rawNumber = invoiceNumberConverter.ConvertToProvider(invoiceNumber);
        object? parsedNumber = invoiceNumberConverter.ConvertFromProvider(rawNumber);

        object? rawMoney = moneyConverter.ConvertToProvider(money);
        object? parsedMoney = moneyConverter.ConvertFromProvider(rawMoney);

        // Assert
        rawEmail.Should().Be("user@aquasmart.io");
        parsedEmail.Should().Be(email);

        rawNumber.Should().Be("INV-2026-00042");
        parsedNumber.Should().Be(invoiceNumber);

        rawMoney.Should().Be(19.95m);
        parsedMoney.Should().Be(money);
    }

    [Fact]
    public void DbContext_ShouldTrackAllBillingAggregates_WithoutConfigurationErrors()
    {
        // Arrange
        using BillingDbContext context = CreateDbContext();

        BillingCustomer customer = new BillingCustomerBuilder().Build();
        customer.AddPaymentMethod(
            Guid.NewGuid(),
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe,
            "pm_test_123",
            "4242",
            "Visa",
            12,
            2028,
            setAsDefault: true);

        Plan plan = new PlanBuilder().Build();
        Subscription subscription = new SubscriptionBuilder().WithCustomerId(customer.Id).WithPlan(plan).Build();
        Invoice invoice = new InvoiceBuilder().WithCustomerId(customer.Id).WithSubscriptionId(subscription.Id).WithDefaultItem().Build();
        Payment payment = new PaymentBuilder().WithCustomerId(customer.Id).WithInvoiceId(invoice.Id).Build();

        // Act
        context.BillingCustomers.Add(customer);
        context.Plans.Add(plan);
        context.Subscriptions.Add(subscription);
        context.Invoices.Add(invoice);
        context.Payments.Add(payment);

        // Assert
        context.ChangeTracker.Entries<BillingCustomer>().Should().ContainSingle();
        context.ChangeTracker.Entries<PaymentMethod>().Should().ContainSingle();
        context.ChangeTracker.Entries<Plan>().Should().ContainSingle();
        context.ChangeTracker.Entries<Subscription>().Should().ContainSingle();
        context.ChangeTracker.Entries<Invoice>().Should().ContainSingle();
        context.ChangeTracker.Entries<InvoiceItem>().Should().ContainSingle();
        context.ChangeTracker.Entries<Payment>().Should().ContainSingle();
    }
}
