using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Control.Infrastructure.Persistence.Converters;

internal class ConditionThresholdConverter : ValueConverter<ConditionThreshold, string>
{
    public ConditionThresholdConverter() : base(
        vo => vo.ToString(),
        dbVal => ConditionThreshold.Parse(dbVal))
    {
    }
}
