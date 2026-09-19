using Device.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Device.Infrastructure.Persistence.Converters;

internal class MacAddressConverter : ValueConverter<MacAddress, string>
{
    public MacAddressConverter() : base(
        vo => vo.Value,
        dbVal => MacAddress.Parse(dbVal))
    {
    }
}
