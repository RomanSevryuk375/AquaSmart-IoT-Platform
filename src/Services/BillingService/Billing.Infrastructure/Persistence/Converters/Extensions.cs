using Billing.Domain.ValueObjects;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Persistence.Converters;

internal static class Extensions
{
    public static ModelConfigurationBuilder AddValueConverters(
        this ModelConfigurationBuilder modelConfigurationBuilder)
    {
        modelConfigurationBuilder.Properties<EmailAddress>()
            .HaveConversion<EmailAddressConverter>()
            .HaveMaxLength(IdentityConstants.EmailLength);

        modelConfigurationBuilder.Properties<InvoiceNumber>()
            .HaveConversion<InvoiceNumberConverter>()
            .HaveMaxLength(InvoiceNumber.MaxLength);

        modelConfigurationBuilder.Properties<Money>()
            .HaveConversion<MoneyConverter>()
            .HavePrecision(12, 2);

        modelConfigurationBuilder.Properties<Enum>()
            .HaveConversion<int>();

        return modelConfigurationBuilder;
    }
}
