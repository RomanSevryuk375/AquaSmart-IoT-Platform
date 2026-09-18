using Control.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Control.Infrastructure.Persistence.Converters;

internal class NameConverter : ValueConverter<Name, string>
{
    public NameConverter() : base(
        vo => vo.Value,
        dbVal => Name.Parse(dbVal))
    {
    }
}
