using Billing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Billing.Infrastructure.Persistence.Converters;

internal sealed class InvoiceNumberConverter : ValueConverter<InvoiceNumber, string>
{
    public InvoiceNumberConverter() : base(
        vo => vo.Value,
        dbVal => InvoiceNumber.Parse(dbVal))
    {
    }
}
