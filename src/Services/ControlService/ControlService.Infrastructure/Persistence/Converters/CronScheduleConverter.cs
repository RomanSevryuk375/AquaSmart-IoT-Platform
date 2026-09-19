using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Control.Infrastructure.Persistence.Converters;

internal class CronScheduleConverter : ValueConverter<CronSchedule, string>
{
    public CronScheduleConverter() : base(
        vo => vo.Value,
        dbVal => CronSchedule.Parse(dbVal))
    {
    }
}
