using BuildingBlocks.Domain.Constants;
using Device.Domain.ValueObjects;

namespace Device.Infrastructure.Persistence.Converters;

internal static class Extensions
{
    public static ModelConfigurationBuilder AddValueConverters(
        this ModelConfigurationBuilder modelConfigurationBuilder)
    {
        modelConfigurationBuilder.Properties<MacAddress>()
            .HaveConversion<MacAddressConverter>()
            .HaveMaxLength(ControllerConstants.MacAddressLength);

        modelConfigurationBuilder.Properties<DeviceName>()
            .HaveConversion<DeviceNameConverter>()
            .HaveMaxLength(CommonConstants.NameLength);


        modelConfigurationBuilder.Properties<ConnectionAddress>()
            .HaveConversion<ConnectionAddressConverter>()
            .HaveMaxLength(ConnectionAddress.MaxLength);

        modelConfigurationBuilder.Properties<Enum>()
            .HaveConversion<int>();

        return modelConfigurationBuilder;
    }
}
