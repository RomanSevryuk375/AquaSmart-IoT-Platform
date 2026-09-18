using Device.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Device.Infrastructure.Persistence.Converters;

internal class DeviceNameConverter : ValueConverter<DeviceName, string>
{
    public DeviceNameConverter() : base(
        vo => vo.Value,
        dbVal => DeviceName.Parse(dbVal))
    {

    }
}
