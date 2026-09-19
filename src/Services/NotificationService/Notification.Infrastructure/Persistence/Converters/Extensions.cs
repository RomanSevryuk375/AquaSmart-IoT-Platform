using BuildingBlocks.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Persistence.Converters;

internal static class Extensions
{
    public static ModelConfigurationBuilder AddValueConverters(
        this ModelConfigurationBuilder modelConfigurationBuilder)
    {
        modelConfigurationBuilder.Properties<Name>()
            .HaveConversion<NameConverter>()
            .HaveMaxLength(ReminderConstants.NameLength);

        modelConfigurationBuilder.Properties<EmailAddress>()
            .HaveConversion<EmailAddressConverter>()
            .HaveMaxLength(UserConstants.EmailLength);

        modelConfigurationBuilder.Properties<PhoneNumber>()
            .HaveConversion<PhoneNumberConverter>()
            .HaveMaxLength(UserConstants.PhoneNumberLength);

        modelConfigurationBuilder.Properties<TimeZoneId>()
            .HaveConversion<TimeZoneIdConverter>()
            .HaveMaxLength(128);

        modelConfigurationBuilder.Properties<MessageText>()
            .HaveConversion<MessageTextConverter>()
            .HaveMaxLength(NotificationConstants.MessageLength);

        modelConfigurationBuilder.Properties<Enum>()
            .HaveConversion<int>();

        return modelConfigurationBuilder;
    }
}
