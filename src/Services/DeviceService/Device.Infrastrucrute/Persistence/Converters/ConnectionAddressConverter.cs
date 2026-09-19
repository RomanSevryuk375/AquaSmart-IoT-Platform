using Device.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Device.Infrastructure.Persistence.Converters;

internal class ConnectionAddressConverter : ValueConverter<ConnectionAddress, string>
{
    public ConnectionAddressConverter() : base(
        vo => vo.ToString(),
        dbVal => ConnectionAddress.Parse(dbVal))
    {
    }
}
