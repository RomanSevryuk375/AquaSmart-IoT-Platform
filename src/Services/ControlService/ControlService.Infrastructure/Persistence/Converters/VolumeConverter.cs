using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Control.Infrastructure.Persistence.Converters;

internal class VolumeConverter : ValueConverter<Volume, double>
{
    public VolumeConverter() : base(
        vo => vo.Value,
        dbVal => Volume.Parse(dbVal))
    {
    }
}
