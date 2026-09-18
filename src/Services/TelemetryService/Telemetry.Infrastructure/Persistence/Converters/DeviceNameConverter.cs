using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Infrastructure.Persistence.Converters;

internal class DeviceNameConverter : ValueConverter<DeviceName, string>
{
    public DeviceNameConverter() : base(
        vo => vo.Value,
        dbVal => DeviceName.Parse(dbVal))
    {
    }
}
