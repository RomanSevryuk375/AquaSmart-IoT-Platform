using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Control.Infrastructure.Persistence.Converters;

internal class DateRangeConverter : ValueConverter<DateRange, string>
{
    public DateRangeConverter() : base(
        vo => vo.ToString(),
        dbVal => DateRange.Parse(dbVal))
    {
    }
}
