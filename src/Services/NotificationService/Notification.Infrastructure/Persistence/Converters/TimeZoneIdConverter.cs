using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Persistence.Converters;

internal class TimeZoneIdConverter : ValueConverter<TimeZoneId, string>
{
    public TimeZoneIdConverter() : base(
        vo => vo.Value,
        dbVal => TimeZoneId.Parse(dbVal))
    {
    }
}
