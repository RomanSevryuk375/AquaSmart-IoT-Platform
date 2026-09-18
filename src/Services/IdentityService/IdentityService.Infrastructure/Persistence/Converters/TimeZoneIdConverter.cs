using IdentityService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdentityService.Infrastructure.Persistence.Converters;

internal class TimeZoneIdConverter : ValueConverter<TimeZoneId, string>
{
    public TimeZoneIdConverter() : base(
        vo => vo.Value,
        dbVal => TimeZoneId.Parse(dbVal))
    {
    }
}
