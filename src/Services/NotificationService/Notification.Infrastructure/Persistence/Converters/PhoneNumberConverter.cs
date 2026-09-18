using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Persistence.Converters;

internal class PhoneNumberConverter : ValueConverter<PhoneNumber, string>
{
    public PhoneNumberConverter() : base(
        vo => vo.Value,
        dbVal => PhoneNumber.Parse(dbVal))
    {
    }
}
