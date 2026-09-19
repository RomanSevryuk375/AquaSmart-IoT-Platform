using BuildingBlocks.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Infrastructure.Persistence.Converters;

internal static class Extensions
{
    public static ModelConfigurationBuilder AddValueConverters(
        this ModelConfigurationBuilder modelConfigurationBuilder)
    {
        modelConfigurationBuilder.Properties<DeviceName>()
            .HaveConversion<DeviceNameConverter>()
            .HaveMaxLength(CommonConstants.NameLength);

        modelConfigurationBuilder.Properties<Enum>()
            .HaveConversion<int>();

        return modelConfigurationBuilder;
    }
}
