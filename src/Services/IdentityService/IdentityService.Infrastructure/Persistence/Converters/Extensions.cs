using BuildingBlocks.Domain.Constants;
using IdentityService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Converters;

internal static class Extensions
{
    public static ModelConfigurationBuilder AddValueConverters(
        this ModelConfigurationBuilder modelConfigurationBuilder)
    {
        modelConfigurationBuilder.Properties<Name>()
            .HaveConversion<NameConverter>()
            .HaveMaxLength(CommonConstants.NameLength);

        modelConfigurationBuilder.Properties<EmailAddress>()
            .HaveConversion<EmailAddressConverter>()
            .HaveMaxLength(UserConstants.EmailLength);

        modelConfigurationBuilder.Properties<PhoneNumber>()
            .HaveConversion<PhoneNumberConverter>()
            .HaveMaxLength(UserConstants.PhoneNumberLength);

        modelConfigurationBuilder.Properties<TimeZoneId>()
            .HaveConversion<TimeZoneIdConverter>()
            .HaveMaxLength(128);

        modelConfigurationBuilder.Properties<Money>()
            .HaveConversion<MoneyConverter>()
            .HavePrecision(18, 2);

        modelConfigurationBuilder.Properties<Enum>()
            .HaveConversion<int>();

        return modelConfigurationBuilder;
    }
}
