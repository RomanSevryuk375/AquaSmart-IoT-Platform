using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Persistence.Converters;

internal class EmailAddressConverter : ValueConverter<EmailAddress, string>
{
    public EmailAddressConverter() : base(
        vo => vo.Value,
        dbVal => EmailAddress.Parse(dbVal))
    {
    }
}
