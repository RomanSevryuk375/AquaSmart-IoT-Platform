using IdentityService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdentityService.Infrastructure.Persistence.Converters;

internal class MoneyConverter : ValueConverter<Money, decimal>
{
    public MoneyConverter() : base(
        vo => vo.Amount,
        dbVal => Money.Parse(dbVal))
    {
    }
}
