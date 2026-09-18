using BuildingBlocks.Domain.Constants;
using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence.Converters;

internal static class Extensions
{
    public static ModelConfigurationBuilder AddValueConverters(
        this ModelConfigurationBuilder modelConfigurationBuilder)
    {
        modelConfigurationBuilder.Properties<Name>()
            .HaveConversion<NameConverter>()
            .HaveMaxLength(CommonConstants.NameLength);

        modelConfigurationBuilder.Properties<Volume>()
            .HaveConversion<VolumeConverter>();

        modelConfigurationBuilder.Properties<ConditionThreshold>()
            .HaveConversion<ConditionThresholdConverter>()
            .HaveMaxLength(64);

        modelConfigurationBuilder.Properties<CronSchedule>()
            .HaveConversion<CronScheduleConverter>()
            .HaveMaxLength(16);

        modelConfigurationBuilder.Properties<DateRange>()
            .HaveConversion<DateRangeConverter>()
            .HaveMaxLength(128);

        modelConfigurationBuilder.Properties<Enum>()
            .HaveConversion<int>();

        return modelConfigurationBuilder;
    }
}
