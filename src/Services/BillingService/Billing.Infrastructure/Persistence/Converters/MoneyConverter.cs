using BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Billing.Infrastructure.Persistence.Converters;

internal sealed class MoneyConverter : ValueConverter<Money, decimal>
{
    public MoneyConverter() : base(
        vo => vo.Amount,
        dbVal => Money.Parse(dbVal))
    {
    }
}
