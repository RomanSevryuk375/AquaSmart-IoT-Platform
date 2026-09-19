using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Persistence.Converters;

internal class NameConverter : ValueConverter<Name, string>
{
    public NameConverter() : base(
        vo => vo.Value,
        dbVal => Name.Parse(dbVal))
    {
    }
}
